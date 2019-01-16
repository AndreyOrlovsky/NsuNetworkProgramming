using System;
using System.Linq;
using System.Windows.Forms;
using SharedData;

namespace RestClient
{
    public partial class FormMain : Form
    {
        Client apiClient = new Client();
        int lastMessageGot = 0;

        public FormMain()
        {
            InitializeComponent();

            FormLogin formLogin = new FormLogin();
            while (!apiClient.IsLoggedIn)
            {
                if (formLogin.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        apiClient.LoginAsync(formLogin.textBoxUsername.Text).Wait();
                    }
                    catch (UsernameTakenException)
                    {
                        MessageBox.Show("Username taken", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't login to the server: " + e.Message, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    Environment.Exit(0);
                    break;
                }
            }

            timerUpdate.Start();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            apiClient.PostMessageAsync(textBoxMessage.Text).Wait();

            textBoxMessage.Text = "";
            textBoxMessage.Focus();
        }

        private void timerUpdateMessages_TickAsync(object sender, EventArgs e)
        {
            var users = apiClient.GetUsersAsync().Result;

            listBoxUsers.Items.Clear();
            foreach (var user in users.Where(u => u.Online == true))
            {
                listBoxUsers.Items.Add(user.Username);
            }
            foreach (var user in users.Where(u => u.Online != true))
            {
                listBoxUsers.Items.Add((user.Online.HasValue ? "[-] " : "[?] ") + user.Username);
            }

            var messages = apiClient.GetMessagesAsync(lastMessageGot).Result;

            lastMessageGot += messages.Count;

            foreach (var message in messages)
            {
                var authorUsermane = users.FirstOrDefault(u => u.Id == message.Author).Username;
                var columns = new string[] { authorUsermane, message.Message };
                var item = new ListViewItem(columns);
                listViewMessages.Items.Add(item);
                item.EnsureVisible();
            }

        }

        private void textBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonSend.PerformClick();
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            apiClient.LogoutAsync().Wait();
        }
    }
}
