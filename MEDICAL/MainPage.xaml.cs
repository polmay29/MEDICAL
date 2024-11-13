using System;
using Microsoft.Maui.Controls;

namespace MEDICAL
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Обработчик кнопки регистрации
        private void OnRegisterClicked(object sender, EventArgs e)
        {
            // Проверка введенных данных
            string fullName = entryFullName.Text?.Trim();
            string oms = entryOms.Text?.Trim();
            string passport = entryPassport.Text?.Trim();
            string email = entryEmail.Text?.Trim();

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(oms) || string.IsNullOrEmpty(passport) || string.IsNullOrEmpty(email))
            {
                DisplayAlert("Ошибка", "Все поля обязательны для заполнения", "OK");
                return;
            }

            // Проверка на правильность формата полиса ОМС (цифры)
            if (!long.TryParse(oms, out _))
            {
                DisplayAlert("Ошибка", "Полис ОМС должен содержать только цифры", "OK");
                return;
            }

            // Проверка на правильность формата паспорта (цифры)
            if (!long.TryParse(passport, out _))
            {
                DisplayAlert("Ошибка", "Паспорт должен содержать только цифры", "OK");
                return;
            }

            // Проверка на правильность формата email
            if (!email.Contains("@"))
            {
                DisplayAlert("Ошибка", "Некорректный формат email", "OK");
                return;
            }

            // Сохранение данных в JSON (можно расширить функциональность)
            Patient newPatient = new Patient
            {
                FullName = fullName,
                Oms = oms,
                Passport = passport,
                BirthDate = datePickerBirthDate.Date,
                Email = email
            };

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filePath = System.IO.Path.Combine(folderPath, "patient.json");

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            var jsonData = System.Text.Json.JsonSerializer.Serialize(newPatient, options);

            System.IO.File.WriteAllText(filePath, jsonData);

            DisplayAlert("Успех", "Пациент успешно зарегистрирован!", "OK");
        }

        // Обработчик кнопки записи к врачу
        private async void OnAppointmentClicked(object sender, EventArgs e)
        {
            // Проверка, что пациент зарегистрирован
            if (string.IsNullOrWhiteSpace(entryFullName.Text))
            {
                await DisplayAlert("Ошибка", "Сначала зарегистрируйтесь!", "OK");
                return;
            }

            // Переход на страницу выбора врача
            await Navigation.PushAsync(new DoctorSelectionPage());
        }
    }

    // Модель данных пациента
    public class Patient
    {
        public string FullName { get; set; }
        public string Oms { get; set; }
        public string Passport { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
    }
}
