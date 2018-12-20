namespace RestClient
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.loginButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // loginButton
            // 
            this.loginButton.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.loginButton.ForeColor = System.Drawing.Color.Black;
            this.loginButton.Location = new System.Drawing.Point(208, 139);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(103, 27);
            this.loginButton.TabIndex = 0;
            this.loginButton.Text = "Enter chat";
            this.loginButton.UseVisualStyleBackColor = false;
            this.loginButton.Click += new System.EventHandler(this.Form1_Load);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F);
            this.label1.ForeColor = System.Drawing.Color.SteelBlue;
            this.label1.Location = new System.Drawing.Point(139, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(253, 31);
            this.label1.TabIndex = 1;
            this.label1.Text = "What is your name?";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(154, 75);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(228, 20);
            this.textBox1.TabIndex = 2;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 243);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.loginButton);
            this.Name = "LoginForm";
            this.Text = "Hello";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
    }
}

