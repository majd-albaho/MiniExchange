import {
  Component, inject, signal, OnInit, OnDestroy,
  ViewChild, ElementRef, AfterViewInit, Input, OnChanges, SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { createChart, IChartApi, ISeriesApi, ColorType, CandlestickData, Time, CandlestickSeries } from 'lightweight-charts';
import { TradeService } from '../../../core/services/trade.service';
import { MatButtonModule } from '@angular/material/button';

const INTERVALS = ['1m', '5m', '15m', '1h', '4h', '1d'];

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
  private chart: IChartApi | null = null;
  private candleSeries: ISeriesApi<'Candlestick'> | null = null;

  selectedInterval = signal('1h');
  intervals = INTERVALS;

  ngAfterViewInit(): void {
    this.initChart();
    this.loadData();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['pair'] && !changes['pair'].firstChange) {
      this.loadData();
    }
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
  }

  changeInterval(iv: string): void {
    this.selectedInterval.set(iv);
    this.loadData();
  }

  ngOnDestroy(): void {
    this.chart?.remove();
  }
}
