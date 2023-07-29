using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace FSofTUtils {

   public partial class MyMessageBox : Form {

      /// <summary>
      /// Ergebnis des Dialoges
      /// </summary>
      DialogResult dialogResult = DialogResult.None;

      /// <summary>
      /// anzuzeigende Info
      /// </summary>
      string Message = null;

      /// <summary>
      /// Titelzeile der <see cref="MyMessageBox"/>
      /// </summary>
      string Title = null;

      /// <summary>
      /// Info-Text zentriert?
      /// </summary>
      bool MessageCentered = false;

      /// <summary>
      /// zu verwendende Buttons
      /// </summary>
      MessageBoxButtons BoxButtons = MessageBoxButtons.OK;

      /// <summary>
      /// Fokus auf diesem Button
      /// </summary>
      MessageBoxDefaultButton BoxDefaultButton = MessageBoxDefaultButton.Button1;

      /// <summary>
      /// zu verwendendes Icon
      /// </summary>
      MessageBoxIcon BoxIcon = MessageBoxIcon.None;

      readonly List<Button> ButtonCollection = new List<Button>();

      /// <summary>
      /// Escape-Taste erlaubt
      /// </summary>
      bool EscapeEnabled = false;

      /// <summary>
      /// Mit Ton-Ausgabe?
      /// </summary>
      bool WithSound = true;


      MyMessageBox() {    // NICHT public -> von "außen" NICHT aufrufbar
         InitializeComponent();
      }

      /// <summary>
      /// zeigt Text-Infos an
      /// <para>Die Parameter entsprechen i.W. <see cref="System.Windows.Forms.MessageBox.Show(string, string, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton)"/>.</para>
      /// </summary>
      /// <param name="msg">anzuzeigender Text</param>
      /// <param name="title">Titelzeile</param>
      /// <param name="buttons">anzuzeigende Buttons</param>
      /// <param name="icon">anzuzeigendes Icon</param>
      /// <param name="defaultButton">Button mit dem Focus</param>
      /// <param name="backcolor">Hintergrundfarbe</param>
      /// <param name="msgCentered">Text linksbündig oder zentriert (mit Umbruch)</param>
      /// <param name="escapeEnabled">Abbruch mit Escap-Taste erlaubt</param>
      /// <param name="withSound">Ton-Ausgabe</param>
      /// <returns>ausgewählter Button oder <see cref="DialogResult.None"/></returns>
      public static DialogResult Show(string msg,
                                      string title,
                                      MessageBoxButtons buttons = MessageBoxButtons.OK,
                                      MessageBoxIcon icon = MessageBoxIcon.None,
                                      MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
                                      Color? backcolor = null,
                                      bool msgCentered = false,
                                      bool escapeEnabled = false,
                                      bool withSound = true) {
         MyMessageBox box = new MyMessageBox {
            Message = msg,
            Title = title,
            MessageCentered = msgCentered,
            BoxButtons = buttons,
            BoxDefaultButton = defaultButton,
            BoxIcon = icon,
            EscapeEnabled = escapeEnabled,
            WithSound = withSound,
         };
         if (backcolor != null)
            box.BackColor = backcolor.GetValueOrDefault();
         box.ShowDialog();
         return box.dialogResult;
      }

      private void MessageBox_Load(object sender, EventArgs e) {
         TopMost = true;

         if (!string.IsNullOrEmpty(Title))
            Text = Title;

         if (BoxIcon == MessageBoxIcon.None) {
            textBoxInfo.Width += textBoxInfo.Left - 5;
            textBoxInfo.Left = 5;
         }
         textBoxInfo.BackColor = BackColor;

         if (!string.IsNullOrEmpty(Message)) {
            if (MessageCentered) {
               textBoxInfo.TextAlign = HorizontalAlignment.Center;   // dann allerdings immer mit WordWrap
               //textBoxInfo.WordWrap = false;
            }
            textBoxInfo.Text = Message;
            textBoxInfo.SelectionStart = 0;
            textBoxInfo.SelectionLength = 0;
         }

         switch (BoxIcon) {
            // Das Meldungsfeld enthält ein Symbol, das aus einem weißen X in einem Kreis mit rotem Hintergrund besteht.
            //Stop
            //Error
            case MessageBoxIcon.Hand:
               pictureBox1.Image = SystemIcons.Hand.ToBitmap();
               if (WithSound)
                  SystemSounds.Hand.Play();
               break;

            // Das Meldungsfeld enthält ein Symbol mit einem Fragezeichen in einem Kreis.
            case MessageBoxIcon.Question:
               pictureBox1.Image = SystemIcons.Question.ToBitmap();
               if (WithSound)
                  SystemSounds.Question.Play();
               break;

            // Das Meldungsfeld enthält ein Symbol, das aus einem Ausrufezeichen in einem Dreieck mit gelben Hintergrund besteht.
            //Warning
            case MessageBoxIcon.Exclamation:
               pictureBox1.Image = SystemIcons.Exclamation.ToBitmap();
               if (WithSound)
                  SystemSounds.Exclamation.Play();
               break;

            // Das Meldungsfeld enthält ein Symbol, das aus dem Kleinbuchstaben „i“ in einem Kreis besteht.
            //Information
            case MessageBoxIcon.Asterisk:
               pictureBox1.Image = SystemIcons.Asterisk.ToBitmap();
               if (WithSound)
                  SystemSounds.Asterisk.Play();
               break;

            default:
               if (WithSound)
                  SystemSounds.Beep.Play();
               break;
         }

         switch (BoxButtons) {
            //     Das Meldungsfeld enthält die Schaltfläche "OK".
            case MessageBoxButtons.OK:
               ButtonCollection.Add(new Button() {
                  Text = "OK",
                  Tag = 0,
               });
               break;

            //     Das Meldungsfeld enthält die Schaltflächen OK und Abbrechen.
            case MessageBoxButtons.OKCancel:
               ButtonCollection.Add(new Button() {
                  Text = "OK",
                  Tag = 0,
               });
               ButtonCollection.Add(new Button() {
                  Text = "abbrechen",
                  Tag = 1,
               });
               break;

            //     Das Meldungsfeld enthält die Schaltflächen Abbrechen, wiederholen und ignorieren.
            case MessageBoxButtons.AbortRetryIgnore:
               ButtonCollection.Add(new Button() {
                  Text = "abbrechen",
                  Tag = 0,
               });
               ButtonCollection.Add(new Button() {
                  Text = "wiederholen",
                  Tag = 1,
               });
               ButtonCollection.Add(new Button() {
                  Text = "ignorieren",
                  Tag = 2,
               });
               break;

            //     Das Meldungsfeld enthält Ja, Nein und Abbrechen (Schaltflächen).
            case MessageBoxButtons.YesNoCancel:
               ButtonCollection.Add(new Button() {
                  Text = "Ja",
                  Tag = 0,
               });
               ButtonCollection.Add(new Button() {
                  Text = "Nein",
                  Tag = 1,
               });
               ButtonCollection.Add(new Button() {
                  Text = "abbrechen",
                  Tag = 2,
               });
               break;

            //     Das Meldungsfeld enthält die Schaltflächen Ja und Nein.
            case MessageBoxButtons.YesNo:
               ButtonCollection.Add(new Button() {
                  Text = "Ja",
                  Tag = 0,
               });
               ButtonCollection.Add(new Button() {
                  Text = "Nein",
                  Tag = 1,
               });
               break;

            //     Das Meldungsfeld enthält die Schaltflächen wiederholen und Abbrechen.
            case MessageBoxButtons.RetryCancel:
               ButtonCollection.Add(new Button() {
                  Text = "wiederholen",
                  Tag = 0,
               });
               ButtonCollection.Add(new Button() {
                  Text = "abbrechen",
                  Tag = 1,
               });
               break;
         }

         for (int i = ButtonCollection.Count - 1; i >= 0; i--) {
            ButtonCollection[i].Click += Button_Click;
            ButtonCollection[i].Font = textBoxInfo.Font;
            ButtonCollection[i].Height = 30;
            ButtonCollection[i].Width = 100;

            flowLayoutPanelButton.Controls.Add(ButtonCollection[i]);
         }

      }

      private void MessageBox_Shown(object sender, EventArgs e) {
         switch (BoxDefaultButton) {
            case MessageBoxDefaultButton.Button1:
               if (ButtonCollection.Count > 0)
                  ButtonCollection[0].Focus();
               break;

            case MessageBoxDefaultButton.Button2:
               if (ButtonCollection.Count > 1)
                  ButtonCollection[1].Focus();
               break;

            case MessageBoxDefaultButton.Button3:
               if (ButtonCollection.Count > 2)
                  ButtonCollection[2].Focus();
               break;
         }

         textBoxInfo_SizeChanged(textBoxInfo, new EventArgs());
      }

      private void Button_Click(object sender, EventArgs e) {
         int buttonno = (int)((sender as Button).Tag);

         switch (BoxButtons) {
            case MessageBoxButtons.OK:
               dialogResult = DialogResult.OK;
               break;

            case MessageBoxButtons.OKCancel:
               switch (buttonno) {
                  case 0:
                     dialogResult = DialogResult.OK;
                     break;

                  case 1:
                     dialogResult = DialogResult.Cancel;
                     break;
               }
               break;

            case MessageBoxButtons.AbortRetryIgnore:
               switch (buttonno) {
                  case 0:
                     dialogResult = DialogResult.Abort;
                     break;

                  case 1:
                     dialogResult = DialogResult.Retry;
                     break;

                  case 2:
                     dialogResult = DialogResult.Ignore;
                     break;
               }
               break;

            case MessageBoxButtons.YesNoCancel:
               switch (buttonno) {
                  case 0:
                     dialogResult = DialogResult.Yes;
                     break;

                  case 1:
                     dialogResult = DialogResult.No;
                     break;

                  case 2:
                     dialogResult = DialogResult.Cancel;
                     break;
               }
               break;

            case MessageBoxButtons.YesNo:
               switch (buttonno) {
                  case 0:
                     dialogResult = DialogResult.Yes;
                     break;

                  case 1:
                     dialogResult = DialogResult.No;
                     break;
               }
               break;

            case MessageBoxButtons.RetryCancel:
               switch (buttonno) {
                  case 0:
                     dialogResult = DialogResult.Retry;
                     break;

                  case 1:
                     dialogResult = DialogResult.Cancel;
                     break;
               }
               break;
         }
         Close();
      }

      private void textBoxInfo_SizeChanged(object sender, EventArgs e) {
         if (!string.IsNullOrEmpty(Message)) {
            TextBox tb = sender as TextBox;

            Graphics g = CreateGraphics();
            SizeF txtSize = g.MeasureString(Message, tb.Font);
            int txtwidth = (int)Math.Ceiling(txtSize.Width);
            int txtheight = (int)Math.Ceiling(txtSize.Height);

            if (txtwidth <= tb.ClientSize.Width &&
                txtheight <= tb.ClientSize.Height)
               tb.ScrollBars = ScrollBars.None;
            else     // notfalls einschalten
               tb.ScrollBars = ScrollBars.Both;
         }
      }

      private void MyMessageBox_KeyDown(object sender, KeyEventArgs e) {
         if (e.KeyCode == Keys.Escape &&
             EscapeEnabled) {
            dialogResult = DialogResult.Cancel;
            Close();
         }
      }
   }
}
