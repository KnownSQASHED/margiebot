﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using Bazam.NoobWebClient;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using Newtonsoft.Json.Linq;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class WeatherRequestResponseProcessor : IResponseProcessor
    {
        private string LastData { get; set; }
        private DateTime? LastDataGrab { get; set; }
        private string WundergroundAPIKey { get; set; }

        public WeatherRequestResponseProcessor()
        {
            WundergroundAPIKey = File.ReadAllText("weather.key");
        }

        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\bweather\b");
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            string data = string.Empty;
            if (LastDataGrab != null && LastDataGrab.Value > DateTime.Now.AddMinutes(-10)) {
                data = LastData;
            }
            else {
                NoobWebClient client = new NoobWebClient();
                data = client.GetResponse("http://api.wunderground.com/api/" + WundergroundAPIKey + "/conditions/q/TN/Nashville.json", RequestType.Get).GetAwaiter().GetResult();
                LastData = data;
                LastDataGrab = DateTime.Now;
            }
            
            JObject jData = JObject.Parse(data);
            if (jData["current_observation"] != null) {
                string tempString = jData["current_observation"]["temp_f"].Value<string>();
                double temp = double.Parse(tempString);

                return new BotMessage() { Text = "It's about " + Math.Round(temp).ToString() + "° out, and it's " + jData["current_observation"]["weather"].Value<string>().ToLower() + ". Not bad, but it ain't hoedown weather, is it?\n\nIf you wanna see more. head over to " + jData["current_observation"]["forecast_url"].Value<string>() + " - my girlfriend DonnaBot works over there!" };
            }
            else {
                return new BotMessage() { Text = "Aww, nuts. My weatherbot gal-pal ain't around. Try 'gin later - she's prolly just fixin' her makeup." };
            }
        }
    }
}