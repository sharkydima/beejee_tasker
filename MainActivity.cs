using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using beejee_Tasker.Src;
using System.Collections.Generic;
using beejee_Tasker.Src.DataStructures;

namespace beejee_Tasker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TaskListAdapter tasklist;
        int page = 1;
        int total_items = 0;
        string orderBy = "id";
        bool orderDesc = false;
        AuthDialog authDialog;
        TaskEditDialog taskEditDialog;

        
        //Для управления стейтами Visible пунктов логина/разлогина
        IMenuItem mnuAuth;
        IMenuItem mnuLogoff;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //Создаем адаптер для отображения
            var lvTasks = FindViewById<ListView>(Resource.Id.lvTasks);
            tasklist = new TaskListAdapter(this, UpdateTasks(true));
            lvTasks.Adapter = tasklist;

            //Инициализируем "хранилище"
            Storage.Instance.Init();

            //Вешаем обработчики событий
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
            FloatingActionButton fabNext = FindViewById<FloatingActionButton>(Resource.Id.fabNextPage);
            FloatingActionButton fabPrev = FindViewById<FloatingActionButton>(Resource.Id.fabPrevPage);
            fabNext.Click += FabNext_Click;
            fabPrev.Click += FabPrev_Click;
            fabPrev.Visibility = ViewStates.Invisible; //Скрываем кнопку предидущей страницы, т.к. стартуем на первой странице.

            RadioButton rb = FindViewById<RadioButton>(Resource.Id.rbSortId);
            rb.Click += Rb_Click; 
            rb = FindViewById<RadioButton>(Resource.Id.rbSortUsename);
            rb.Click += Rb_Click;
            rb = FindViewById<RadioButton>(Resource.Id.rbSortEmail);
            rb.Click += Rb_Click;
            rb = FindViewById<RadioButton>(Resource.Id.rbSortStatus);
            rb.Click += Rb_Click;

            lvTasks.ItemClick += LvTasks_Click;

            //Инстансим диалоги и задаем действия при скрытии диалогов
            authDialog = new AuthDialog(this);
            authDialog.OnHide(() => { //При скрытии диалога авторизации проверяем была ли она выполнена. И если да, то меняем пункты меню
                if (!string.IsNullOrEmpty(Storage.Instance.GetToken())) {
                    mnuAuth.SetVisible(false);
                    mnuLogoff.SetVisible(true);
                }
            });
            taskEditDialog = new TaskEditDialog(this);
            taskEditDialog.OnHide((modified) => { //При скрытии диалога выполняем обновление данных списка задач в случае наличия изменений.
                if (modified)
                {
                    if (taskEditDialog.IsNewItem) //Если создали новую задачу, тогда перезагружаем данные с текущими параметрами сортировки.
                    {
                        tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
                    }
                    tasklist.NotifyDataSetChanged();
                }
            });
        }

        private void LvTasks_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            if(string.IsNullOrEmpty(Storage.Instance.GetToken()))
            {
                View view = (View)sender;
                Snackbar.Make(view, "Need to be logged in", Snackbar.LengthLong)
                    .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
                return;
            }
            var task = tasklist.ListSource[e.Position];
            taskEditDialog.SetEditItem(task);
            taskEditDialog.Show();
        }

        /// <summary>
        /// Нажатие на радиокнопки сортировки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rb_Click(object sender, EventArgs e)
        {
            if (!(sender is RadioButton))
                return;
            var rb = sender as RadioButton;
            switch (rb.Id) //Смотрим на какую радиокнопку было нажатие
            {
                case Resource.Id.rbSortId:
                    if (orderBy == "id") //Если по нажатой ранее, то меняем направление на противоположное
                    {
                        orderDesc = !orderDesc;
                    }
                    else //Иначе меняем поле сортировки и направление на ascending
                    {
                        orderBy = "id";
                        orderDesc = false;
                    }
                    break;
                case Resource.Id.rbSortUsename:
                    if (orderBy == "username")
                    {
                        orderDesc = !orderDesc;
                    }
                    else
                    {
                        orderBy = "username";
                        orderDesc = false;
                    }
                    break;
                case Resource.Id.rbSortEmail:
                    if (orderBy == "email")
                    {
                        orderDesc = !orderDesc;
                    }
                    else
                    {
                        orderBy = "email";
                        orderDesc = false;
                    }
                    break;
                case Resource.Id.rbSortStatus:
                    if (orderBy == "status")
                    {
                        orderDesc = !orderDesc;
                    }
                    else
                    {
                        orderBy = "status";
                        orderDesc = false;
                    }
                    break;
            }

            string s = rb.Text.Trim('↑', '↓');
            s += orderDesc ? '↓' : '↑';
            rb.Text = s;

            tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
        }


        private void FabPrev_Click(object sender, EventArgs e)
        {
            if (page <= 1)
            {
                Toast.MakeText(this, "That is a first page", ToastLength.Long).Show();
                return;
            }
            page--;
            tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
            if (page <= 1)
                FindViewById<FloatingActionButton>(Resource.Id.fabPrevPage).Visibility = ViewStates.Invisible;
            if(page < (total_items / 3 + ((total_items % 3 > 0) ? 1 : 0)))
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Visible;
        }

        private void FabNext_Click(object sender, EventArgs e)
        {
           if (page >= (total_items / 3 + ((total_items % 3 > 0) ? 1 : 0)))
            {
                Toast.MakeText(this, "That is a last page", ToastLength.Long).Show();
                return;
            }
            page++;
            tasklist.ListSource = UpdateTasks(false) ?? tasklist.ListSource;
            if(page >= (total_items / 3 + ((total_items % 3 > 0) ? 1 : 0))) //Вычисление количества страниц в данной процедуре делается дважды, т.к. оно может измениться в процессе работы (обновление данных на сервере)
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Invisible;
            if(page > 1)
                FindViewById<FloatingActionButton>(Resource.Id.fabPrevPage).Visibility = ViewStates.Visible;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            //Сохраняем ссылки на пункты меню для дальнейшего использования
            mnuAuth = menu.GetItem(0);
            mnuLogoff = menu.GetItem(1);

            //И выставляем сразу активный пункт в зависимости от данных о сохраненной авторизации. В данном случае используются 2 независимых пункта для действий входа/выхоа
            if (string.IsNullOrEmpty(Storage.Instance.GetToken()))
            {
                mnuAuth.SetVisible(true);
                mnuLogoff.SetVisible(false);
            }
            else
            {
                mnuAuth.SetVisible(false);
                mnuLogoff.SetVisible(true);
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_auth)
            {
                authDialog.Show();                
                return true;
            } 
            else if(id == Resource.Id.action_logoff)
            {
                
                Storage.Instance.ResetAuthToken();
                Toast.MakeText(this, "Deauthed...", ToastLength.Long).Show(); 
                mnuAuth.SetVisible(true);
                mnuLogoff.SetVisible(false);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            taskEditDialog.SetEditItem(null);
            taskEditDialog.Show();
        }
        
        List<TaskData> UpdateTasks(bool bExitOnFail)
        {
            //Получаем таски
            var tasks = ApiLayer.GetTasks(page, orderBy, orderDesc);
            if (tasks.result != "ok")
            {
                Toast.MakeText(this, tasks.result, ToastLength.Long).Show();
                if (bExitOnFail) //Если задан флаг bExitOnFail и нет связи или произошла ошибка, ждем 5сек и выходим.
                {
                    System.Threading.Thread.Sleep(5000);
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                }
                return null; 
            }

            total_items = tasks.total_task_count;
            if(total_items < 3) //Если задач мало, то прячем кнопку пагинации
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Invisible;
            else
                FindViewById<FloatingActionButton>(Resource.Id.fabNextPage).Visibility = ViewStates.Visible;
            return tasks.tasks;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        
	}
}
