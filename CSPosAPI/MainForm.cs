﻿using CSPosAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace CSPosAPI
{
    public partial class MainForm : Form
    {

        private double summaryNonCash;
        private double summaryAmount;
        private double summaryVat;
        private double summaryCityTax;
        PrintDocument pdoc = null;
        private Result resultData;

        private List<BillBankTransaction> ListBankTranscation;

        public MainForm()
        {
            InitializeComponent();
             
            //this.dataGridViewStocks.Rows[0].Cells["Code"].Value = "10001";
            //this.dataGridViewStocks.Rows[0].Cells["Name"].Value = "Талх";
            //this.dataGridViewStocks.Rows[0].Cells["MeasureUnit"].Value = "ш";
            //this.dataGridViewStocks.Rows[0].Cells["Qty"].Value = "3.00";
            //this.dataGridViewStocks.Rows[0].Cells["UnitPriceNonVat"].Value = "1000.00";
            //this.dataGridViewStocks.Rows[0].Cells["UnitPrice"].Value = "1100.00";
            //this.dataGridViewStocks.Rows[0].Cells["Amount"].Value = "3300.00";
            //this.dataGridViewStocks.Rows[0].Cells["Vat"].Value = "300";
            //this.dataGridViewStocks.Rows[0].Cells["BarCode"].Value = "564889656";
            //this.dataGridViewStocks.Rows[0].Cells["CityTax"].Value = "0.00";
            
            
        }

        private void buttonNonCash_Click(object sender, EventArgs e)
        {
            var formNonCash = new FormNonCash();
            formNonCash.ShowDialog();
            this.summaryNonCash = formNonCash.SummaryNonCash;
            textBoxNonCash.Text = this.summaryNonCash.ToString(Program.NUMBER_FORMAT);
            this.ListBankTranscation = formNonCash.ListBankTranscation;
            Calculate();
            this.summaryNonCash = 0;
        }
        private void buttonCreateBill_Click(object sender, EventArgs e)
        {

            var data = new BillData();
            data.amount = textBoxAmount.Text;
            data.vat = textBoxVat.Text;
            data.cashAmount = textBoxCash.Text;
            data.nonCashAmount = textBoxNonCash.Text;
            data.billIdSuffix = textBoxNumber.Text;

            var lstBillStock = new List<BillDetail>();

            foreach (DataGridViewRow row in dataGridViewStocks.Rows)
            {
                if (!row.IsNewRow)
                {
                    var stock = new BillDetail();
                    stock.code = row.Cells["Code"].Value.ToString();
                    stock.name = row.Cells["ItemName"].Value.ToString();
                    stock.measureUnit = row.Cells["MeasureUnit"].Value.ToString();
                    stock.qty = row.Cells["Qty"].Value.ToString();
                    stock.unitPrice = row.Cells["UnitPriceNonVat"].Value.ToString();
                    stock.totalAmount = row.Cells["Amount"].Value.ToString();
                    stock.vat = row.Cells["Vat"].Value.ToString();
                    stock.barCode = row.Cells["BarCode"].Value.ToString();
                    stock.cityTax = row.Cells["CityTax"].Value.ToString();
                    lstBillStock.Add(stock);
                }
            }
            data.cityTax = textBoxCityTax.Text;
            data.bankTransactions = ListBankTranscation;
            //?????
            if (lstBillStock.Count == 0)
            {
                lstBillStock=null;
            }
            data.stocks = lstBillStock;
            var json = new JavaScriptSerializer().Serialize(data);
            var result = Program.put(json);


            this.resultData = new JavaScriptSerializer().Deserialize<Result>(result);
            
            if ("True".Equals(this.resultData.success.ToString()))
            {
                print();
            }
        }

        private void buttonReturnBill_Click(object sender, EventArgs e)
        {
            var formBillReturn = new BillReturn();
            formBillReturn.ShowDialog();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Program.sendData());
        }

        private void dataGridViewStocks_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            summaryAmount = 0;
            summaryVat = 0;
            summaryCityTax = 0;
            foreach (DataGridViewRow row in dataGridViewStocks.Rows)
            {
                if (!row.IsNewRow)
                {
                    double qty = Convert.ToDouble(row.Cells["Qty"].Value);
                    double unitPrice = Convert.ToDouble(row.Cells["UnitPrice"].Value);
                    double vat = Convert.ToDouble(row.Cells["Vat"].Value);
                    double cityTax = Convert.ToDouble(row.Cells["CityTax"].Value);
                    double unitPriceNonVat = Convert.ToDouble(row.Cells["UnitPriceNonVat"].Value);

                    double summary = qty * unitPrice;
                    row.Cells["UnitPrice"].Value = (unitPriceNonVat * 1.1).ToString(Program.NUMBER_FORMAT);
                    row.Cells["Vat"].Value = (unitPriceNonVat * 0.1).ToString(Program.NUMBER_FORMAT);
                    row.Cells["Amount"].Value = summary.ToString(Program.NUMBER_FORMAT);
                    summaryAmount += summary;
                    summaryVat += vat;
                    summaryCityTax += cityTax;

                }
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (comboBoxStock.SelectedItem != null)
            {
                var stock = GetItemById(comboBoxStock.SelectedItem.ToString());
                DataGridViewRow row = (DataGridViewRow)dataGridViewStocks.Rows[0].Clone();
                row.Cells[0].Value = stock.code;
                row.Cells[1].Value = stock.name;
                row.Cells[2].Value = stock.measureUnit;
                row.Cells[3].Value = stock.qty ;
                row.Cells[4].Value = stock.unitPrice ;
                row.Cells[5].Value =( Convert.ToDouble(stock.unitPrice) * Convert.ToDouble(stock.qty)).ToString();
                row.Cells[6].Value = stock.totalAmount ;
                row.Cells[7].Value = stock.vat ;
                row.Cells[8].Value = stock.barCode ;
                row.Cells[9].Value = stock.cityTax  ;
                this.dataGridViewStocks.Rows.Add(row);
                Calculate();
                Calculate();
            }
           
           
        }
        private BillDetail GetItemById(string id) {
            var stock = new BillDetail();

            if ("1201".Equals(id)) {
                stock.code =id;
                stock.name = "Талх";
                stock.measureUnit = "ш";
                stock.qty = "3.00";
                stock.unitPrice = "1000.00";
                stock.totalAmount = "3900";
                stock.vat = "100";
                stock.barCode = "156266";
                stock.cityTax = "0.00";
            }
            else if("1202".Equals(id)){
                stock.code = id;
                stock.name = "Сүү";
                stock.measureUnit = "л";
                stock.qty = "1.00";
                stock.unitPrice = "1000.00";
                stock.totalAmount = "1000.00";
                stock.vat = "100.00";
                stock.barCode = "156266";
                stock.cityTax = "0.00";
            }
            else if ("1000".Equals(id))
            {
                stock.code = id;
                stock.name = "Сүү";
                stock.measureUnit = "л";
                stock.qty = "1.00";
                stock.unitPrice = "9.80";
                stock.totalAmount = "980.00";
                stock.vat = "100.00";
                stock.barCode = "156266";
                stock.cityTax = "0.00";
            }
            else if ("1001".Equals(id))
            {
                stock.code = id;
                stock.name = "Соёмбо Архи";
                stock.measureUnit = "ш";
                stock.qty = "1.00";
                stock.unitPrice = "40000.00";
                stock.totalAmount = "40000.00";
                stock.vat = "4000.00";
                stock.barCode = "0124652";
                stock.cityTax = "4500.00";
            }
            else if ("1002".Equals(id))
            {
                stock.code = id;
                stock.name = "Гуляш";
                stock.measureUnit = "ш";
                stock.qty = "1.00";
                stock.unitPrice = "4000.00";
                stock.totalAmount = "4000.00";
                stock.vat = "400.00";
                stock.barCode = "01246526";
                stock.cityTax = "0.00";
            }
            else if ("2001".Equals(id))
            {
                stock.code = id;
                stock.name = "Тамхи Esse";
                stock.measureUnit = "ш";
                stock.qty = "1.00";
                stock.unitPrice = "4000.00";
                stock.totalAmount = "4000.00";
                stock.vat = "400.00";
                stock.barCode = "012465233";
                stock.cityTax = "500.00";
            }
            else if ("2002".Equals(id))
            {
                stock.code = id;
                stock.name = "Тамхи Esse";
                stock.measureUnit = "ш";
                stock.qty = "1.00";
                stock.unitPrice = "4000.00";
                stock.totalAmount = "4000.00";
                stock.vat = "400.00";
                stock.barCode = "012465233";
                stock.cityTax = "500.00";
            }

            return stock;
        }

        private void Calculate() {
            dataGridViewStocks_CellValueChanged(null, null);
            textBoxAmount.Text = summaryAmount.ToString(Program.NUMBER_FORMAT);
            textBoxVat.Text = summaryVat.ToString(Program.NUMBER_FORMAT);
            textBoxCityTax.Text = summaryCityTax.ToString(Program.NUMBER_FORMAT);
            textBoxNonCash.Text = summaryNonCash.ToString(Program.NUMBER_FORMAT);
            textBoxCash.Text = (summaryAmount - summaryNonCash < 0 ? 0 : summaryAmount - summaryNonCash).ToString(Program.NUMBER_FORMAT);
        }

        public void print()
        {
            PrintDialog pd = new PrintDialog();
            pdoc = new PrintDocument();
            PrinterSettings ps = new PrinterSettings();
            Font font = new Font("Courier New", 15);


            PaperSize psize = new PaperSize("Custom", 100, 200);
            //ps.DefaultPageSettings.PaperSize = psize;
            
            pd.Document = pdoc;
            pd.Document.DefaultPageSettings.PaperSize = psize;
            //pdoc.DefaultPageSettings.PaperSize.Height =320;
            pdoc.DefaultPageSettings.PaperSize.Height = 820;

            pdoc.DefaultPageSettings.PaperSize.Width = 520;

            pdoc.PrintPage += new PrintPageEventHandler(pdoc_PrintPage);

            DialogResult result = pd.ShowDialog();
            if (result == DialogResult.OK)
            {
                PrintPreviewDialog pp = new PrintPreviewDialog();
                pp.Document = pdoc;
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
                pp.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
                result = pp.ShowDialog();
                if (result == DialogResult.OK)
                {
                    pdoc.Print();
                }
            }

        }

        void pdoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Font font = new Font("Courier New", 10);
            float fontHeight = font.GetHeight();
            int startX = 50;
            int startY = 55;
            int Offset = 40;
       
            graphics.DrawString("Мерчант : " + this.resultData.merchantId,
                    new Font("Courier New", 11),
                    new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + 20;

            graphics.DrawString("Баримтын дугаар: \n" + this.resultData.billId,
                     new Font("Courier New", 11),
                     new SolidBrush(Color.Black), new Rectangle(startX , startY + Offset, 400 , 200));
            Offset = Offset + 20 +20;
            graphics.DrawString("Огноо : " + this.resultData.date,
                     new Font("Courier New", 11),
                     new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + 20;

            if (resultData.lottery != null && resultData.lottery.Length != 0)
            {
                graphics.DrawString("Сугалаа : " + this.resultData.lottery,
                    new Font("Courier New", 11),
                    new SolidBrush(Color.Black), startX, startY + Offset);
                Offset = Offset + 20;
            }

            String underLine = "------------------------------------------";

            graphics.DrawString(underLine, new Font("Courier New", 10),
                     new SolidBrush(Color.Black), startX, startY + Offset);

            Offset = Offset + 20;

            graphics.DrawString("Бараа " + " тоо/ш" + " Үнэ " + " НӨАТ/орсон " + " City/tax " + " Дүн", new Font("Courier New", 10),
                     new SolidBrush(Color.Black), startX, startY + Offset);

            Offset = Offset + 20;

            if (this.resultData.stocks != null)
            {
                foreach (BillDetail stock in this.resultData.stocks)
                {
                    string unitPriceVat = (Convert.ToDouble(stock.unitPrice) + Convert.ToDouble(stock.vat)).ToString();
                    graphics.DrawString(stock.name + " " + stock.qty + "  " + stock.unitPrice + " " + unitPriceVat + "  " + stock.cityTax + " " + stock.totalAmount, new Font("Courier New", 10),
                       new SolidBrush(Color.Black), startX, startY + Offset);
                    Offset = Offset + 20;
                }

            }

            underLine = "------------------------------------------";
            graphics.DrawString(underLine, new Font("Courier New", 10),
                     new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + 20;

            if (resultData.bankTransactions != null && resultData.bankTransactions.Count != 0) {

                graphics.DrawString("Банк/нэр" + "   RRN   " + "   Approval   " + "   Дүн" , new Font("Courier New", 10),
                  new SolidBrush(Color.Black), startX, startY + Offset);

                Offset = Offset + 20;
                foreach (BillBankTransaction banktranscation in this.resultData.bankTransactions) {
                    graphics.DrawString(banktranscation.bankName + " " + banktranscation.rrn + " " + banktranscation.approvalCode + "  " + banktranscation.amount, new Font("Courier New", 10),
                      new SolidBrush(Color.Black), startX, startY + Offset);
                    Offset = Offset + 20;
                }

                underLine = "------------------------------------------";
                graphics.DrawString(underLine, new Font("Courier New", 10),
                         new SolidBrush(Color.Black), startX, startY + Offset);
                Offset = Offset + 20;
            }

            graphics.DrawString("Бэлэн : " + resultData.cashAmount, new Font("Courier New", 10),
                   new SolidBrush(Color.Black), startX, startY + Offset);

            Offset = Offset + 20;

            graphics.DrawString("Бэлэн Бус : " + resultData.nonCashAmount, new Font("Courier New", 10),
                   new SolidBrush(Color.Black), startX, startY + Offset);

            Offset = Offset + 20;

            graphics.DrawString("Нийт : " + resultData.amount, new Font("Courier New", 10),
                   new SolidBrush(Color.Black), startX, startY + Offset);

            Offset = Offset + 20;

            graphics.DrawString("НӨАТ : " + resultData.vat, new Font("Courier New", 10),
                 new SolidBrush(Color.Black), startX, startY + Offset);

            Offset = Offset + 20;

            graphics.DrawString("НХОАТ : " + resultData.cityTax, new Font("Courier New", 10),
               new SolidBrush(Color.Black), startX, startY + Offset);

            Offset = Offset + 20;

            underLine = "------------------------------------------";
            graphics.DrawString(underLine, new Font("Courier New", 10),
                     new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + 20;

            if (resultData.lottery != null && resultData.lottery.Length != 0)
            {
                ZXing.IBarcodeWriter writer = new ZXing.BarcodeWriter { Format = ZXing.BarcodeFormat.QR_CODE };
                var dataQr = writer.Write(resultData.qrData);
                var barcodeBitmap = new Bitmap(dataQr,new Size(150,150));
                graphics.DrawImage(barcodeBitmap, startX + 120, startY + Offset); 

                Offset = Offset + 100 + 50;
            }
            if (resultData.internalCode != null && resultData.internalCode.Length != 0)
            {
                graphics.DrawString("Internal Code : ", new Font("Courier New", 10),
                       new SolidBrush(Color.Black), startX + 70, startY + Offset);
                Offset = Offset + 20;

                graphics.DrawString(resultData.internalCode, new Font("Courier New", 10),
                        new SolidBrush(Color.Black),new Rectangle(startX +70, startY + Offset , 200, 50) );
                Offset = Offset + 20;
            }
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            dataGridViewStocks.Rows.Clear();
            Calculate();
        }

    }
}