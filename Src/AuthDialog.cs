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

namespace beejee_Tasker.Src
{
    //Диалог авторизации
    class AuthDialog
    {
        //Я использую такой подход поскольку мне удобнее разрабатывать и отлаживать структурированные по функционалу объекты чем гору кода в одном классе. 
        //Так-же такой подход в реализиции интерфейса мне показался удобнее с точки зрения пользователей, в отличии от отдельных страниц. (согласно опросу пользователей одного из моих приложений)
        Activity activity;
        View dialog_view;
        AlertDialog dlg;
        AlertDialog.Builder builder;

        EditText etUsername;
        EditText etPassword;

        Button btnLogin;
        Action onHide;

        public AuthDialog(Activity activity)
        {
            this.activity = activity;
            dialog_view = activity.LayoutInflater.Inflate(Resource.Layout.AuthDialog, null);
            etUsername = dialog_view.FindViewById<EditText>(Resource.Id.etUsername);
            etPassword = dialog_view.FindViewById<EditText>(Resource.Id.etPassword);
            btnLogin = dialog_view.FindViewById<Button>(Resource.Id.btnLogin);

            btnLogin.Click += BtnLogin_Click;

            builder = new AlertDialog.Builder(activity);
            builder.SetView(dialog_view);
            dlg = builder.Create();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string token;
            //Простейшая влидация полей.
            if(string.IsNullOrWhiteSpace(etUsername.Text))
            {
                Toast.MakeText(activity, "User name: This field is required", ToastLength.Long).Show();
                return;
            }
            if(string.IsNullOrWhiteSpace(etPassword.Text))
            {
                Toast.MakeText(activity, "Password: This field is required", ToastLength.Long).Show();
                return;
            }

            string result = ApiLayer.LogIn(etUsername.Text, etPassword.Text, out token);
            if(result != "ok")
            { //Выводим сообщение об ошибке авторизации, но не закрываем диалог.
                Toast.MakeText(activity, result, ToastLength.Long).Show();
                return;
            }

            Storage.Instance.SetTokenData(token);

            Hide();
            Toast.MakeText(activity, "Auth success", ToastLength.Long).Show();
        }

        //Такие функции обычно делаются в абстрактном родительском классе, но т.к. она еденична, то я не вижу смысла плодить классы.
        public void OnHide(Action action)
        {
            onHide = action;
        }

        public void Show()
        {
            etPassword.Text = "";
            dlg.Show();
        }

        public void Hide()
        {
            dlg.Hide();
            if (onHide != null)
                onHide();
        }
    }
}