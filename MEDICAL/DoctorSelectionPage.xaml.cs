
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MEDICAL
{
    public partial class DoctorSelectionPage : ContentPage
    {
        private List<Doctor> doctors = new List<Doctor>();

        public DoctorSelectionPage()
        {
            InitializeComponent();
            LoadDoctors();
        }

        // ����� ��� �������� ������ �� JSON
        private async void LoadDoctors()
        {
            try
            {
                var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var filePath = Path.Combine(folderPath, "C:\\Users\\User\\source\\repos\\MEDICAL\\MEDICAL\\bin\\Debug\\net8.0-windows10.0.19041.0\\win10-x64\\doctors.json");

                Console.WriteLine($"File Path: {filePath}");

                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var doctorList = JsonConvert.DeserializeObject<DoctorList>(json);
                    if (doctorList != null)
                    {
                        doctors = doctorList.Doctors;
                        var specialties = doctors.Select(d => d.Specialization).Distinct().ToList();
                        specialtyPicker.ItemsSource = specialties;
                    }
                }
                else
                {
                    await DisplayAlert("������", $"�� ������� ����� ���� � �������. ����: {filePath}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"��������� ������ ��� �������� ������: {ex.Message}", "OK");
            }
        }

        // ���������� ������ �������������
        private void specialtyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedSpecialty = specialtyPicker.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedSpecialty))
            {
                var filteredDoctors = doctors.Where(d => d.Specialization == selectedSpecialty).Select(x => new { x.Name, x.AvailableTimes }).ToList();

                doctorPicker.ItemsSource = filteredDoctors.Select(x => x.Name).ToList();
                doctorPicker.SelectedItem = null;
                doctorPicker.IsEnabled = true;
                timePicker.ItemsSource = null;
                timePicker.IsEnabled = false;
            }
        }

        // ���������� ������ �����
        private void doctorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedDoctorName = doctorPicker.SelectedItem as string;
            var selectedDoctor = doctors.FirstOrDefault(d => d.Name == selectedDoctorName);

            if (selectedDoctor != null)
            {
                timePicker.ItemsSource = selectedDoctor.AvailableTimes;
                timePicker.SelectedItem = null;
                timePicker.IsEnabled = true;
            }
        }

        // ���������� ������ ����
        private void datePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            // ����� �������� ������ ��� ��������� ����
            Console.WriteLine($"������� ����: {e.NewDate.ToShortDateString()}");
        }

        // ���������� ������ ������ �� �����
        private async void OnBookAppointmentClicked(object sender, EventArgs e)
        {
            var selectedDoctorName = doctorPicker.SelectedItem as string;
            var selectedDoctor = doctors.FirstOrDefault(d => d.Name == selectedDoctorName);
            var selectedTime = timePicker.SelectedItem as string;
            var selectedDate = datePicker.Date;
            var email = emailEntry.Text;

            if (selectedDoctor == null)
            {
                await DisplayAlert("������", "�������� �����", "OK");
                return;
            }

            if (selectedTime == null)
            {
                await DisplayAlert("������", "�������� ����� ��� ������", "OK");
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                await DisplayAlert("������", "������� email", "OK");
                return;
            }

            string appointmentMessage = $"�� �������� � ����� {selectedDoctor.Name} ({selectedDoctor.Specialization}) �� {selectedDate.ToShortDateString()} � {selectedTime}";

            bool emailSent = await SendEmailConfirmation(email, appointmentMessage);

            if (emailSent)
            {
                await DisplayAlert("������ ������������", appointmentMessage, "OK");
            }
            else
            {
                await DisplayAlert("������", "�� ������� ��������� ������������� �� email", "OK");
            }
        }

        // ����� ��� �������� email
        private async Task<bool> SendEmailConfirmation(string toEmail, string message)
        {
            try
            {
                var fromAddress = new MailAddress("Polmay29@yandex.ru", "Medical App");
                var toAddress = new MailAddress(toEmail);
                const string subject = "������������� ������ �� �����";

                using (var smtp = new SmtpClient("smtp.yandex.com"))
                {
                    // ��������� SMTP
                    smtp.Port = 587;  // ���� ��� TLS
                    smtp.Credentials = new NetworkCredential("Polmay29@yandex.ru", "zpyzhzunuccucmif");  // ���������, ��� ������ ����������
                    smtp.EnableSsl = true;  // �������� SSL ��� ����������� ����������
                    smtp.Timeout = 10000;  // ����-��� ��� �������� ������

                    // ������� ������
                    var mailMessage = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = false  // ���������, ��� ���� ������ � ������� plain text
                    };

                    // ���������� ���������� ������
                    smtp.Send(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"������ ��� �������� ������: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                await DisplayAlert("������", $"�� ������� ��������� ������������� �� email: {ex.Message}", "OK");
                return false;
            }
        }

        public class Doctor
        {
            public string Name { get; set; }
            public string Specialization { get; set; }
            public List<string> AvailableTimes { get; set; }
        }

        public class DoctorList
        {
            public List<Doctor> Doctors { get; set; }
        }
    }
}
