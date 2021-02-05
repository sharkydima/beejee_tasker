using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace beejee_Tasker.Src.DataStructures
{
    class TaskData
    {
        //Согласно правилам именования в C# названия свойств должны начинаться с заглавной буквы. Для совместимости с json тут объявлены атрибуты с именами полей.
        [JsonProperty(PropertyName ="id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        public TaskData()
        {
            Id = -1; //Инициализация с идентификатором -1 для отслеживания новых экземпляров TaskData
        }
    }
}