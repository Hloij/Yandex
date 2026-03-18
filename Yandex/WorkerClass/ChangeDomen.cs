//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.DirectoryServices.ActiveDirectory;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Timers;
//using Yandex.Model.ModelsBD;
//using YandexAds;
//using YandexOneprofit;


//namespace Yandex.WorkerClass
//{
//    class ChangeDomen
//    {
//        System.Timers.Timer timer = new();
       
//        public ChangeDomen()
//        {
           
//           // timer.Interval = 300000; // Интервал в миллисекундах
//              timer.Interval = 100000; // Интервал в миллисекундах

//            // Подписываем событие Elapsed (происходит каждый интервал)
//            timer.Elapsed += Change;

//            // Включаем таймер
//            timer.Enabled = true;
//            timer.Start();
//        }

//        private void Change(object? sender, ElapsedEventArgs e)
//        {
//            if (_userOptions.CanChangeDomen == "True")
//            {
//                Root respons = Api<Root>.GetApiRespons("https://oneprofit.net/api/v1/setting/purchase-domains?project_id=1&region_id=1", _userOptions.TokenOneProfit).Result;
//                ModelAds ads = Api<ModelAds>.GetApiRespons("https://adv-api.adprofex.com/api/ads", _userOptions.TokenAdprofex).Result;
//                int index = 0;
//                for (int i = 0; i < ads.data.Length; i++)
//                {
//                    if (!ads.data[i].Url.AbsoluteUri.Contains(respons.data[0].name))
//                    {
//                        //PosterNews news = new PosterNews();
//                        //news.Title = ads.data[i].Title;
//                        //news.cost = ads.data[i].CustomCost.ToString();
//                        //if (ads.data[i].Description != null)
//                        //{

//                        //    news.description = ads.data[i].Description.ToString();
//                        //}

//                        //news.url = ads.data[i].Url.AbsoluteUri.Replace(domen.Text, newdomen.Text);


//                        //(string, bool) rez = ApiServesNew.PostFormData("https://adv-api.adprofex.com/api/ads", AllColection.osnovOptions[0].TokenAdProfex, news, ads.data[i].Id);
//                        //if (rez.Item2 == true)
//                        //{
//                        //    index++;
//                        //}
//                    }
//                }
//            }
//        }


//    }
//}
//namespace YandexAds
//{
//    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
//    public class PosterNews
//    {
//        public string Title { get; set; }

//        public string image_rectangle { get; set; }
//        public string image_square { get; set; }
//        public string description { get; set; }
//        public string url { get; set; }
//        public string cost { get; set; }
//        public string compainId { get; set; }
//    }
//    public class ModelAds
//    {
//        public Ads[] data { get; set; }
//        public Compain[] campaigns { get; set; }
//        public Statuses statuses { get; set; }
//        public bool simpleStatistic { get; set; }
//    }
//    public class Compain
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }
//        [JsonProperty("name")]
//        public string? Name { get; set; }
//        [JsonProperty("status")]
//        public Status? Status { get; set; }
//    }
//    public class Statuses
//    {
//        [JsonProperty("moderation")]
//        public moder[] moderation { get; set; }
//        [JsonProperty("ads")]
//        public moder[] ads { get; set; }
//    }

//    public class moder
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }
//        [JsonProperty("name")]
//        public string? Name { get; set; }
//    }

//    public class Ads
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("title")]
//        public string? Title { get; set; }

//        [JsonProperty("description")]
//        public object? Description { get; set; }

//        [JsonProperty("url")]
//        public Uri? Url { get; set; }

//        [JsonProperty("limit")]
//        public long? Limit { get; set; }

//        [JsonProperty("limited")]
//        public bool? Limited { get; set; }

//        [JsonProperty("image")]
//        public Image? Image { get; set; }

//        [JsonProperty("icon")]
//        public Icon? Icon { get; set; }

//        [JsonProperty("video")]
//        public Video? Video { get; set; }

//        [JsonProperty("custom_cost")]
//        public double CustomCost { get; set; }

//        [JsonProperty("ab_tests_exists")]
//        public bool? AbTestsExists { get; set; }

//        [JsonProperty("cpa_lead_price")]
//        public object? CpaLeadPrice { get; set; }

//        [JsonProperty("cpa_lead_currency")]
//        public object? CpaLeadCurrency { get; set; }

//        [JsonProperty("ad_type")]
//        public AdType? AdType { get; set; }

//        [JsonProperty("decline_reason")]
//        public object? DeclineReason { get; set; }

//        [JsonProperty("cpa_status")]
//        public CpaStatus? CpaStatus { get; set; }

//        [JsonProperty("cost_type")]
//        public CostType? CostType { get; set; }

//        [JsonProperty("status")]
//        public Status? Status { get; set; }

//        [JsonProperty("moderation_status")]
//        public ModerationStatus? ModerationStatus { get; set; }

//        [JsonProperty("balance_status")]
//        public BalanceStatus? BalanceStatus { get; set; }

//        [JsonProperty("age_rating")]
//        public AgeRating? AgeRating { get; set; }

//        [JsonProperty("time_range_status")]
//        public TimeRangeStatus? TimeRangeStatus { get; set; }

//        [JsonProperty("recommendation")]
//        public Recommendation? Recommendation { get; set; }

//        [JsonProperty("status_tooltip")]
//        public StatusTooltip? StatusTooltip { get; set; }

//        [JsonProperty("ch")]
//        public Ch? Ch { get; set; }

//        [JsonProperty("created_at")]
//        public DateTimeOffset? CreatedAt { get; set; }

//        [JsonProperty("updated_at")]
//        public DateTimeOffset? UpdatedAt { get; set; }

//        [JsonProperty("ad_type_id")]
//        public long? AdTypeId { get; set; }

//        [JsonProperty("campaign_id")]
//        public long? CampaignId { get; set; }

//        [JsonProperty("cost_type_id")]
//        public long? CostTypeId { get; set; }

//        [JsonProperty("status_id")]
//        public long? StatusId { get; set; }

//        [JsonProperty("moderation_status_id")]
//        public long? ModerationStatusId { get; set; }

//        [JsonProperty("balance_status_id")]
//        public long? BalanceStatusId { get; set; }

//        [JsonProperty("age_rating_id")]
//        public long? AgeRatingId { get; set; }

//        [JsonProperty("time_range_status_id")]
//        public long? TimeRangeStatusId { get; set; }

//        [JsonProperty("accelerate_status_id")]
//        public long? AccelerateStatusId { get; set; }

//        [JsonProperty("is_show_push_hint")]
//        public bool? IsShowPushHint { get; set; }
//    }

//    public class Image
//    {
//        [JsonProperty("url1")]
//        public Uri? Url1 { get; set; }

//        [JsonProperty("url2")]
//        public Uri? Url2 { get; set; }
//    }

//    public class Icon
//    {
//        [JsonProperty("url1")]
//        public object? Url1 { get; set; }
//    }

//    public class Video
//    {
//        [JsonProperty("url")]
//        public object? Url { get; set; }
//    }

//    public class AdType
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("slug")]
//        public string? Slug { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }
//    }

//    public class CpaStatus
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }

//        [JsonProperty("slug")]
//        public string? Slug { get; set; }

//        [JsonProperty("color")]
//        public string? Color { get; set; }
//    }

//    public class CostType
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }

//        [JsonProperty("description")]
//        public string? Description { get; set; }
//    }

//    public class Status
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("slug")]
//        public string? Slug { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }

//        [JsonProperty("color")]
//        public string? Color { get; set; }
//    }

//    public class ModerationStatus
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("slug")]
//        public string? Slug { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }

//        [JsonProperty("color")]
//        public string? Color { get; set; }
//    }

//    public class BalanceStatus
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }

//        [JsonProperty("color")]
//        public string? Color { get; set; }
//    }

//    public class AgeRating
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }
//    }

//    public class TimeRangeStatus
//    {
//        [JsonProperty("id")]
//        public long? Id { get; set; }

//        [JsonProperty("name")]
//        public string? Name { get; set; }

//        [JsonProperty("color")]
//        public string? Color { get; set; }
//    }

//    public class Recommendation
//    {
//        [JsonProperty("cpc")]
//        public long? Cpc { get; set; }

//        [JsonProperty("recommendations")]
//        public object[] Recommendations { get; set; }

//        [JsonProperty("other_recommendations")]
//        public object[] OtherRecommendations { get; set; }

//        [JsonProperty("processing")]
//        public bool? Processing { get; set; }

//        [JsonProperty("show_bar")]
//        public bool? ShowBar { get; set; }

//        [JsonProperty("status_text")]
//        public string? StatusText { get; set; }

//        [JsonProperty("traffic_coverage")]
//        public long? TrafficCoverage { get; set; }
//    }

//    public class StatusTooltip
//    {
//        [JsonProperty("primary")]
//        public Primary? Primary { get; set; }

//        [JsonProperty("additional")]
//        public Additional[]? Additional { get; set; }
//    }

//    public class Primary
//    {
//        [JsonProperty("color")]
//        public string? Color { get; set; }

//        [JsonProperty("text")]
//        public string? Text { get; set; }
//    }

//    public class Additional
//    {
//        [JsonProperty("color")]
//        public string? Color { get; set; }

//        [JsonProperty("group")]
//        public string? Group { get; set; }

//        [JsonProperty("text")]
//        public string? Text { get; set; }
//    }

//    public class Ch
//    {
//        [JsonProperty("views_count")]
//        public long? ViewsCount { get; set; }

//        [JsonProperty("clicks_count")]
//        public long? ClicksCount { get; set; }

//        [JsonProperty("ctr")]
//        public long? Ctr { get; set; }

//        [JsonProperty("consumption")]
//        public long? Consumption { get; set; }

//        [JsonProperty("conversion_count")]
//        public long? ConversionCount { get; set; }
//    }

//}
//namespace YandexOneprofit
//{
//    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
//    public class Datum
//    {
//        public int id { get; set; }
//        public string name { get; set; }
//        public object description { get; set; }
//        public object ds { get; set; }
//        public int is_ext_arbitrator { get; set; }
//        public int project_id { get; set; }
//        public int user_id { get; set; }
//        public int weight { get; set; }
//        public int is_spare { get; set; }
//        public string domain_server { get; set; }
//        public int region_id { get; set; }
//        public int is_dont_update_at_banned { get; set; }
//        public int domain_provider_id { get; set; }
//        public string region_name { get; set; }
//        public string user_info { get; set; }
//    }

//    public class Root
//    {
//        public List<Datum> data { get; set; }
//    }
//}


