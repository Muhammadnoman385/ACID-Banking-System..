using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ACID
{
    public partial class Form1 : Form
    {
        // Connection string ko class level par rakha hai taake har jagah use ho sakay
        string connString = "Data Source=AcidBank.db";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // SQLite ko initialize karna zaroori hai
            SQLitePCL.Batteries.Init();
            try
            {
                using (var conn = new SqliteConnection(connString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();

                    // Portfolio ke liye do tables: Accounts aur TransactionHistory
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Accounts (
                            Id INTEGER PRIMARY KEY,
                            Name TEXT,
                            Balance INTEGER CHECK(Balance >= 0)
                        );
                        CREATE TABLE IF NOT EXISTS TransactionHistory (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Sender TEXT,
                            Receiver TEXT,
                            Amount INTEGER,
                            Status TEXT,
                            Timestamp DATETIME
                        );
                        INSERT OR IGNORE INTO Accounts (Id, Name, Balance) VALUES (1, 'Noman', 10000), (2, 'Client', 5000);";
                    cmd.ExecuteNonQuery();
                }
                RefreshBalances();
                LoadHistory(); // App start hotay hi purani history dikhayein
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message);
            }
        }

        private void RefreshBalances()
        {
            try
            {
                using (var conn = new SqliteConnection(connString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT Name, Balance FROM Accounts ORDER BY Id";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["Name"].ToString();
                            string balance = reader["Balance"].ToString();

                            if (name == "Noman") txtNomanBalance.Text = balance;
                            if (name == "Client") txtClientBalance.Text = balance;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Refresh Error: " + ex.Message);
            }
        }

        // GridView mein history dikhane ka function
        private void LoadHistory()
        {
            try
            {
                using (var conn = new SqliteConnection(connString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    // Sabse naye records uupar dikhane ke liye ORDER BY Id DESC
                    cmd.CommandText = "SELECT Id, Sender, Receiver, Amount, Status, Timestamp FROM TransactionHistory ORDER BY Id DESC";

                    DataTable dt = new DataTable();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }

                    // Grid mein data load karna
                    dgvHistory.DataSource = dt;

                    // --- Professional Portfolio Styling ---
                    if (dgvHistory.Columns.Count > 0)
                    {
                        // Columns ke naam behtar karein
                        dgvHistory.Columns["Sender"].HeaderText = "Bhejne Wala";
                        dgvHistory.Columns["Receiver"].HeaderText = "Wasool Karne Wala";
                        dgvHistory.Columns["Amount"].HeaderText = "Raqam";
                        dgvHistory.Columns["Status"].HeaderText = "Halat";
                        dgvHistory.Columns["Timestamp"].HeaderText = "Waqt";

                        // Alignment aur Readability
                        dgvHistory.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 245, 255); // Halki blue line
                        dgvHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        dgvHistory.ReadOnly = true; // User grid mein change na kar sakay
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("History Load Error: " + ex.Message);
            }
        }

        // --- SUCCESS CASE ---
        private void btnSuccess_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAmount.Text)) return;
            if (!int.TryParse(txtAmount.Text, out int amount)) return;

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Noman se minus
                        var cmd1 = conn.CreateCommand();
                        cmd1.Transaction = transaction;
                        cmd1.CommandText = $"UPDATE Accounts SET Balance = Balance - {amount} WHERE Name = 'Noman'";
                        cmd1.ExecuteNonQuery();

                        // Step 2: Client mein jama
                        var cmd2 = conn.CreateCommand();
                        cmd2.Transaction = transaction;
                        cmd2.CommandText = $"UPDATE Accounts SET Balance = Balance + {amount} WHERE Name = 'Client'";
                        cmd2.ExecuteNonQuery();

                        // Step 3: History mein 'SUCCESS' record karein
                        var cmdLog = conn.CreateCommand();
                        cmdLog.Transaction = transaction;
                        cmdLog.CommandText = "INSERT INTO TransactionHistory (Sender, Receiver, Amount, Status, Timestamp) " +
                                             "VALUES ('Noman', 'Client', @amt, 'SUCCESS', DATETIME('now'))";
                        cmdLog.Parameters.AddWithValue("@amt", amount);
                        cmdLog.ExecuteNonQuery();

                        transaction.Commit();
                        MessageBox.Show("Mubarak ho! Transaction Successful.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Transaction Fail: " + ex.Message);
                    }
                }
            }
            RefreshBalances();
            LoadHistory();
        }

        // --- CRASH / FAIL CASE ---
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAmount.Text)) return;
            if (!int.TryParse(txtAmount.Text, out int amount)) return;

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Noman ke paise katne ki koshish
                        var cmd1 = conn.CreateCommand();
                        cmd1.Transaction = transaction;
                        cmd1.CommandText = $"UPDATE Accounts SET Balance = Balance - {amount} WHERE Name = 'Noman'";
                        cmd1.ExecuteNonQuery();

                        // 2. Zabardasti Crash Simulation
                        throw new Exception("System Crash Simulation!");

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // ACID: Rollback sab kuch purani halat par le aayega
                        transaction.Rollback();
                        MessageBox.Show("Crash Hua! Rollback ki wajah se balance aur history dono bach gaye.");
                    }
                }
            }
            RefreshBalances();
            LoadHistory();
        }

        private void dgvHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvHistory.Columns["Sender"].HeaderText = "Bhejne Wala";
            dgvHistory.Columns["Receiver"].HeaderText = "Wasool Karne Wala";
            dgvHistory.Columns["Amount"].HeaderText = "Raqam";
            dgvHistory.Columns["Status"].HeaderText = "Halat";
        }

        private void txtAmount_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtClientBalance_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void txtNomanBalance_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}