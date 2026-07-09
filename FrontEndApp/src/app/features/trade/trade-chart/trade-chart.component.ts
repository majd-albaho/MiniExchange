import {
  Component, inject, signal, effect, OnChanges, SimpleChanges,
  ViewChild, ElementRef, AfterViewInit, Input, OnDestroy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { createChart, IChartApi, ISeriesApi, ColorType, CandlestickData, Time, CandlestickSeries } from 'lightweight-charts';
import { TradeService } from '../../../core/services/trade.service';
import { MarketDataHubService } from '../../../core/services/market-data-hub.service';
import { MatButtonModule } from '@angular/material/button';

const INTERVALS = ['1m', '5m', '15m', '1h', '4h', '1d'];

const INTERVAL_SECONDS: Record<string, number> = {
  '1m': 60,
  '5m': 5 * 60,
  '15m': 15 * 60,
  '1h': 60 * 60,
  '4h': 4 * 60 * 60,
  '1d': 24 * 60 * 60,
};

@Component({
  selector: 'app-trade-chart',
  standalone: true,
  imports: [CommonModule, MatButtonModule],
  templateUrl: './trade-chart.component.html',
  styleUrl: './trade-chart.component.css',
})
export class TradeChartComponent implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('chartRef') chartRef!: ElementRef<HTMLDivElement>;
  @Input() pair = 'BTCUSDT';

  private tradeService = inject(TradeService);
  private hubService = inject(MarketDataHubService);
  private chart: IChartApi | null = null;
  private candleSeries: ISeriesApi<'Candlestick'> | null = null;
  private activeSymbol = signal(this.pair);
  private lastCandle: { time: number; open: number; high: number; low: number } | null = null;

  selectedInterval = signal('1h');
  intervals = INTERVALS;
  livePrice = signal<number | null>(null);
  priceDirection = signal<'up' | 'down' | null>(null);

  constructor() {
    effect(() => {
      const symbol = this.activeSymbol();
      const tick = this.hubService.prices()[symbol];
      if (tick) {
        this.applyTick(tick.lastPrice);
      }
    });
  }

  ngAfterViewInit(): void {
    this.initChart();
    this.subscribeToPair(this.pair);
    this.loadData();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['pair'] && !changes['pair'].firstChange) {
      const previous = changes['pair'].previousValue as string;
      this.hubService.unsubscribe(previous);
      this.subscribeToPair(this.pair);
      this.loadData();
    }
  }

  private subscribeToPair(symbol: string): void {
    this.activeSymbol.set(symbol);
    this.livePrice.set(null);
    this.priceDirection.set(null);
    this.hubService.subscribe(symbol);
  }

  private initChart(): void {
    const el = this.chartRef.nativeElement;
    this.chart = createChart(el, {
      width: el.clientWidth,
      height: el.clientHeight || 360,
      layout: {
        background: { type: ColorType.Solid, color: '#0d1117' },
        textColor: '#9ca3af',
      },
      grid: {
        vertLines: { color: '#1f2937' },
        horzLines: { color: '#1f2937' },
      },
      crosshair: { mode: 1 },
      rightPriceScale: { borderColor: '#374151' },
      timeScale: { borderColor: '#374151', timeVisible: true },
    });

    this.candleSeries = this.chart.addSeries(CandlestickSeries, {
      upColor: '#4caf50',
      downColor: '#f44336',
      borderUpColor: '#4caf50',
      borderDownColor: '#f44336',
      wickUpColor: '#4caf50',
      wickDownColor: '#f44336',
    });

    const observer = new ResizeObserver(() => {
      this.chart?.applyOptions({ width: el.clientWidth, height: el.clientHeight || 360 });
    });
    observer.observe(el);
  }

  async loadData(): Promise<void> {
    const candles = await this.tradeService.getCandles(this.pair, this.selectedInterval());
    const data: CandlestickData[] = candles.map(c => ({
      time: c.time as Time,
      open: c.open, high: c.high, low: c.low, close: c.close,
    }));
    this.candleSeries?.setData(data);
    this.chart?.timeScale().fitContent();

    const last = candles[candles.length - 1];
    this.lastCandle = last ? { time: last.time, open: last.open, high: last.high, low: last.low } : null;
  }

  private applyTick(price: number): void {
    const previous = this.livePrice();
    this.livePrice.set(price);
    if (previous !== null) {
      this.priceDirection.set(price > previous ? 'up' : price < previous ? 'down' : this.priceDirection());
    }

    if (!this.candleSeries || !this.lastCandle) {
      return;
    }

    const intervalSeconds = INTERVAL_SECONDS[this.selectedInterval()] ?? 60;
    const bucketStart = Math.floor(Date.now() / 1000 / intervalSeconds) * intervalSeconds;

    if (bucketStart > this.lastCandle.time) {
      this.lastCandle = { time: bucketStart, open: price, high: price, low: price };
    } else {
      this.lastCandle.high = Math.max(this.lastCandle.high, price);
      this.lastCandle.low = Math.min(this.lastCandle.low, price);
    }

    this.candleSeries.update({
      time: this.lastCandle.time as Time,
      open: this.lastCandle.open,
      high: this.lastCandle.high,
      low: this.lastCandle.low,
      close: price,
    });
  }

  changeInterval(iv: string): void {
    this.selectedInterval.set(iv);
    this.loadData();
  }

  ngOnDestroy(): void {
    this.hubService.unsubscribe(this.activeSymbol());
    this.chart?.remove();
  }
}
