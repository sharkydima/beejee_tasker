using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using beejee_Tasker.Src.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace beejee_Tasker.Src
{
    //Адаптер для массива задач
    class TaskListAdapter : BaseAdapter
    {
        protected Context ctx;
        protected List<TaskData> list;

        public override int Count => (list == null ? 0 : list.Count);
        public List<TaskData> ListSource { 
            get => list;
            set { list = value; NotifyDataSetChanged(); }
            }
        
        public TaskListAdapter(Context context, List<TaskData> dataList)
        {
            ctx = context;
            list = dataList;
        }
        
        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return list[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView ?? LayoutInflater.FromContext(this.ctx).Inflate(Resource.Layout.TaskDataRow, parent, false);
            TextView tvUserEmail = view.FindViewById<TextView>(Resource.Id.tvUsernameEmail);
            TextView tvText = view.FindViewById<TextView>(Resource.Id.tvTaskText);
            TextView tvStatus = view.FindViewById<TextView>(Resource.Id.tvTaskStatus);

            var row = this.list[position];

            tvUserEmail.Text = string.Format("{0} / {1}", row.Username, row.Email);
            tvText.Text = row.Text;
            tvStatus.Text = GetStatusText(row.Status);
            return view;
        }


        private static string GetStatusText(int status)
        {
            //Тут я использовал произвольные статусы, т.к. в документации к API нет конкретных указаний для диапазона статусов 1-9
            string statusText = string.Empty;
            if (status == 0)
                statusText = "New";
            else if (status > 0 && status < 5)
                statusText = "New / Admin modified";
            else if (status == 5)
                statusText = "Started";
            else if (status > 5 && status < 9)
                statusText = "Started / Admin modified";
            else if (status == 9)
                statusText = "Completed / Admin modified";
            else
                statusText = "Completed";
            return statusText;
        }
    }
}