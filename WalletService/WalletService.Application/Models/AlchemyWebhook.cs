namespace WalletService.Application.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class AlchemyWebhookPayload
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("event")]
        public AlchemyEvent? Event { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("webhookId")]
        public string WebhookId { get; set; } = string.Empty;
    }

    public class AlchemyEvent
    {
        [JsonPropertyName("activity")]
        public List<AlchemyActivity>? Activity { get; set; }

        [JsonPropertyName("network")]
        public string Network { get; set; } = string.Empty;
    }

    public class AlchemyActivity
    {
        [JsonPropertyName("asset")]
        public string Asset { get; set; } = string.Empty;

        [JsonPropertyName("blockNum")]
        public string BlockNum { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("erc1155Metadata")]
        public object? Erc1155Metadata { get; set; }

        [JsonPropertyName("erc721TokenId")]
        public string? Erc721TokenId { get; set; }

        [JsonPropertyName("fromAddress")]
        public string FromAddress { get; set; } = string.Empty;

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;

        [JsonPropertyName("log")]
        public AlchemyLog? Log { get; set; }

        [JsonPropertyName("rawContract")]
        public RawContract? RawContract { get; set; }

        [JsonPropertyName("toAddress")]
        public string ToAddress { get; set; } = string.Empty;

        [JsonPropertyName("typeTraceAddress")]
        public string? TypeTraceAddress { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    public class AlchemyLog
    {
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("blockHash")]
        public string BlockHash { get; set; } = string.Empty;

        [JsonPropertyName("blockNumber")]
        public string BlockNumber { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;

        [JsonPropertyName("logIndex")]
        public string LogIndex { get; set; } = string.Empty;

        [JsonPropertyName("removed")]
        public bool Removed { get; set; }

        [JsonPropertyName("topics")]
        public List<string>? Topics { get; set; }

        [JsonPropertyName("transactionHash")]
        public string TransactionHash { get; set; } = string.Empty;

        [JsonPropertyName("transactionIndex")]
        public string TransactionIndex { get; set; } = string.Empty;
    }

    public class RawContract
    {
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("decimals")]
        public int Decimals { get; set; }

        [JsonPropertyName("rawValue")]
        public string RawValue { get; set; } = string.Empty;
    }
}
