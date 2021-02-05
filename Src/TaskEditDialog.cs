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
    class TaskEditDialog
    {
        Activity ctx;
        View dialog_view;

        bool createNew;
        bool haveChanges;
        TaskData editableItem;

        AlertDialog dlg;
        AlertDialog.Builder builder;

        Action<bool> onHide;
        EditText etUsername;
        TextView tvUsername;
        EditText etEmail;
        TextView tvEmail;
        EditText etTaskText;
        CheckBox cbxCompleted;
        Button btnSave;

        public bool IsNewItem { get => createNew; }

        public TaskEditDialog(Activity activity)
        {
            ctx = activity;
            dialog_view = activity.LayoutInflater.Inflate(Resource.Layout.TaskEditDialog, null);

            etUsername = dialog_view.FindViewById<EditText>(Resource.Id.etUsername);
            tvUsername = dialog_view.FindViewById<TextView>(Resource.Id.tvUsername);
            etEmail = dialog_view.FindViewById<EditText>(Resource.Id.etEmail);
            tvEmail = dialog_view.FindViewById<TextView>(Resource.Id.tvEmail);
            etTaskText = dialog_view.FindViewById<EditText>(Resource.Id.etTaskText);
            cbxCompleted = dialog_view.FindViewById<CheckBox>(Resource.Id.cbxCompleted);
            btnSave = dialog_view.FindViewById<Button>(Resource.Id.btnSave);

            btnSave.Click += BtnSave_Click;

            builder = new AlertDialog.Builder(activity);
            builder.SetView(dialog_view);
            dlg = builder.Create();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if(createNew)
            {
                //Простейшая влидация полей.
                if (string.IsNullOrWhiteSpace(etUsername.Text))
                {
                    Toast.MakeText(ctx, "User name: This field is required", ToastLength.Long).Show();
                    return;
                }
                if (string.IsNullOrWhiteSpace(etEmail.Text))
                {
                    Toast.MakeText(ctx, "E-Mail: This field is required", ToastLength.Long).Show();
                    return;
                } else if(!etEmail.Text.Contains('@') || (etEmail.Text.IndexOf('.', etEmail.Text.IndexOf('@')) < 0)) //Простейшая проверка, смысл в том, что проверяем на наличие символа '@', а затем точки после него (доменное имя)
                {
                    Toast.MakeText(ctx, "E-Mail: Please enter valid E-Mail address", ToastLength.Long).Show();
                    return;
                }
                if (string.IsNullOrWhiteSpace(etTaskText.Text))
                {
                    Toast.MakeText(ctx, "Task text: This field is required", ToastLength.Long).Show();
                    return;
                }

                var newItem = ApiLayer.CreateTask(etUsername.Text, etEmail.Text, etTaskText.Text);
                if(newItem.Id == -1) //Ошибка
                {
                    if(!string.IsNullOrEmpty(newItem.Username))
                        Toast.MakeText(ctx, "User name: " + newItem.Username, ToastLength.Long).Show();
                    if (!string.IsNullOrEmpty(newItem.Email))
                        Toast.MakeText(ctx, "E-Mail: " + newItem.Email, ToastLength.Long).Show();
                    if (!string.IsNullOrEmpty(newItem.Text))
                        Toast.MakeText(ctx, "Task text: " + newItem.Text, ToastLength.Long).Show();
                    return;
                }
                Toast.MakeText(ctx, "Task successfully added!", ToastLength.Long).Show();
                haveChanges = true;
                Hide();
            } else
            {
                haveChanges = false;
                if(editableItem.Text != etTaskText.Text)
                {
                    editableItem.Text = etTaskText.Text;
                    haveChanges = true;
                }
                if(cbxCompleted.Checked)
                {
                    if(editableItem.Status != 9)
                    {
                        if (haveChanges) //Если текст был изменен, то ставим признак что таск завершен и отредактирован
                            editableItem.Status = 9; //Completed / Admin edited
                        else
                            editableItem.Status = 10; //Completed
                        haveChanges = true;
                    }
                } else
                {
                    if(editableItem.Status != 1)
                    {
                        editableItem.Status = 1; //New / Admin edited
                        haveChanges = true;
                    }
                }
                if(haveChanges)
                    ApiLayer.EditTask(Storage.Instance.GetToken(), editableItem.Id, editableItem.Text, editableItem.Status);
                Hide();
            }
        }

        public void SetEditItem(TaskData item)
        {
            haveChanges = false;
            if(item == null) //Создание нового...
            {
                createNew = true;
                editableItem = new TaskData();
                cbxCompleted.Visibility = ViewStates.Invisible;
                etUsername.Visibility = ViewStates.Visible;
                tvUsername.Visibility = ViewStates.Visible;
                etEmail.Visibility = ViewStates.Visible;
                tvEmail.Visibility = ViewStates.Visible;
            } 
            else //Редактирование...
            {
                createNew = false;
                editableItem = item;
                cbxCompleted.Visibility = ViewStates.Visible;
                etUsername.Visibility = ViewStates.Invisible;
                tvUsername.Visibility = ViewStates.Invisible;
                etEmail.Visibility = ViewStates.Invisible;
                tvEmail.Visibility = ViewStates.Invisible;
                etTaskText.Text = editableItem.Text;
                cbxCompleted.Checked = editableItem.Status >= 9;
            }

        }

        public void Show()
        {            
            dlg.Show();
        }

        /// <summary>
        /// Задает действие при скрытии диалога
        /// </summary>
        /// <param name="action">Действие, в качестве параметра bool, значение true говорит о том, что были внесены изменения в данные.</param>
        public void OnHide(Action<bool> action)
        {
            onHide = action;
        }

        public void Hide()
        {
            dlg.Hide();
            if (onHide != null)
                onHide(haveChanges);
        }
    }
}