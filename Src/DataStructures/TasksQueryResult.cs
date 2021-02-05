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
    class TasksQueryResult
    {
        public string result { get; set; }
        public List<TaskData> tasks { get; set; }
        public int total_task_count { get; set; }
    }
}