using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_PC_02_Module3
{
    public partial class FormPayment : Form
    {
        DB_PC_02_Module3Entities db = new DB_PC_02_Module3Entities();
        public int discount { get; set; }
        public bool useDiscount { get; set; }
        public string customeroke { get; set; }
        public int tableoke { get; set; }
        header_order data = new header_order();
        promotion promo = new promotion();
        public FormPayment()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelDate.Text = DateTime.Now.ToLongDateString();
            labelTime.Text = DateTime.Now.ToLongTimeString();
        }
        void LoadData()
        {
            var paid = db.payments
                    .ToArray()
                    .Select(x => new
                    {
                        x.id,
                        x.header_order_id,
                        x.header_order.order_made_time,
                        x.header_order.table_number
                    }).ToArray();

            if (cbSort.SelectedIndex == 0)
            {
                dgvPayment.DataSource = db.header_order.ToArray()
                    .OrderBy(a => a.customer_name)
                    .Where(c => !paid.Select(a => a.header_order_id).Contains(c.id))
                    .Select(x => new
                    {
                        x.id,
                        Customer = "Table " + x.table_number + " - " + x.customer_name
                    }).ToArray();
                dgvPayment.Columns[0].Visible = false;
            } else if (cbSort.SelectedIndex == 1)
            {
                dgvPayment.DataSource = db.header_order.ToArray()
                    .OrderByDescending(a => a.customer_name)
                    .Where(c => !paid.Select(a => a.header_order_id).Contains(c.id))
                   .Select(x => new
                   {
                       x.id,
                       Customer = "Table " + x.table_number + " - " + x.customer_name
                   }).ToArray();
                dgvPayment.Columns[0].Visible = false;
            }
            else if (cbSort.SelectedIndex == 2)
            {
                dgvPayment.DataSource = db.header_order.ToArray()
                     .OrderBy(a => a.table_number)
                     .Where(c => !paid.Select(a => a.header_order_id).Contains(c.id))
                    .Select(x => new
                    {
                        x.id,
                        Customer = "Table " + x.table_number + " - " + x.customer_name
                    }).ToArray();
                dgvPayment.Columns[0].Visible = false;
            }
            else if (cbSort.SelectedIndex == 3)
            {
                dgvPayment.DataSource = db.header_order.ToArray()
                     .OrderByDescending(a => a.table_number)
                     .Where(c => !paid.Select(a => a.header_order_id).Contains(c.id))
                    .Select(x => new
                    {
                        x.id,
                        Customer = "Table " + x.table_number + " - " + x.customer_name
                    }).ToArray();
                dgvPayment.Columns[0].Visible = false;
            }


        }
        private void FormPayment_Load(object sender, EventArgs e)
        {
            cbSort.SelectedIndex = 0;
            LoadData();
        }

        private void cbSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void dgvPayment_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            try
            {
                int id = int.Parse(dgvPayment[0, e.RowIndex].Value.ToString());
                tableoke = db.header_order.Find(id).table_number;
                customeroke = db.header_order.Find(id).customer_name;
                data = db.header_order.Find(id);
                dgvDetail.DataSource = db.detail_order
                    .ToArray()
                    .Where(x => x.header_order_id == id)
                    .Select(a => new
                    {
                        a.id,
                        a.menu.name,
                        a.quantity,
                        a.order_price
                    }).ToArray();
                dgvDetail.Columns[0].Visible = false;

                var detail = db.detail_order
                  .ToArray()
                  .Where(x => x.header_order_id == id)
                  .Select(a => new
                  {
                      a.id,
                      a.order_placed_time,
                      a.menu.name,
                      a.quantity,
                      a.order_price
                  }).ToArray();

               
                decimal hargaa = detail.Sum(a => a.order_price);
                int harga = Convert.ToInt32(hargaa);
                labelSubtotal.Text = harga.ToString();
                calculate();

            }
            catch (Exception)
            {
                MessageBox.Show("Something wrong. Maybe, data is invalid. Please try again", "Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
          
        }
        void calculate()
        {
            var sekarang = DateTime.Now;
            var exist = db.promotions
                .FirstOrDefault(x => x.code == txtPromo.Text && x.start_time < sekarang && x.end_time > sekarang);
            if (exist != null)
            {
                decimal minimum = exist.minimum_spent;
                int minimumspend = Convert.ToInt32(minimum);
                if (Convert.ToInt32(labelSubtotal.Text) > minimumspend)
                {
                    promo = exist;
                    decimal disc = exist.discount;
                    int diskon = Convert.ToInt32(disc);
                    discount = diskon;
                    int hargasetelah = Convert.ToInt32(labelSubtotal.Text) * discount / 100;
                    labelDisc.Text = hargasetelah.ToString();
                    labeldisc2.Text = "(" + discount.ToString() + "%)";
                    int subtotal = Convert.ToInt32(labelSubtotal.Text);

                    int pajak = subtotal * 10 / 100;
                    int charge = subtotal * 5 / 100;
                    labelTax.Text = pajak.ToString();
                    labelService.Text = charge.ToString();

                  
                    int total = (subtotal - hargasetelah) + pajak + charge;
                    labelTotal.Text = total.ToString();
                }
                else
                {
                    promo = null;
                    discount = 0;
                    labeldisc2.Text = " ";
                    labelDisc.Text = "0";
                    int hargasetelah = Convert.ToInt32(labelSubtotal.Text);
                    labeldisc2.Text = "(" + discount.ToString() + "%)";
                    int pajak = hargasetelah * 10 / 100;
                    int charge = pajak * 5 / 100;
                    labelTax.Text = pajak.ToString();
                    labelService.Text = charge.ToString();
                    int subtotal = Convert.ToInt32(labelSubtotal.Text);
                    int total = subtotal + pajak + charge;
                    labelTotal.Text = total.ToString();
                }
            }
            else
            {
                promo = null;
                discount = 0;
                labeldisc2.Text = " ";
                labelDisc.Text = "0";
                int hargasetelah = Convert.ToInt32(labelSubtotal.Text);
                labeldisc2.Text = "(" + discount.ToString() + "%)";
                int pajak = hargasetelah * 10 / 100;
                int charge = pajak * 5 / 100;
                labelTax.Text = pajak.ToString();
                labelService.Text = charge.ToString();
                int subtotal = Convert.ToInt32(labelSubtotal.Text);
                int total = subtotal + pajak + charge;
                labelTotal.Text = total.ToString();
            }
        }
        private void txtPromo_TextChanged(object sender, EventArgs e)
        {
            var sekarang = DateTime.Now;
            var exist = db.promotions
                .FirstOrDefault(x => x.code == txtPromo.Text && x.start_time < sekarang && x.end_time > sekarang);
            if (exist != null)
            {
                decimal minimum = exist.minimum_spent;
                int minimumspend = Convert.ToInt32(minimum);
                if (Convert.ToInt32(labelSubtotal.Text) > minimumspend)
                {
                    promo = exist;
                    decimal disc = exist.discount;
                    int diskon = Convert.ToInt32(disc);
                    discount = diskon;
                    int hargasetelah = Convert.ToInt32(labelSubtotal.Text) * discount / 100;
                    labelDisc.Text = hargasetelah.ToString();
                    labeldisc2.Text = "(" + discount.ToString() + "%)";
                    int subtotal = Convert.ToInt32(labelSubtotal.Text);

                    int pajak = subtotal * 10 / 100;
                    int charge = subtotal * 5 / 100;
                    labelTax.Text = pajak.ToString();
                    labelService.Text = charge.ToString();


                    int total = (subtotal - hargasetelah) + pajak + charge;
                    labelTotal.Text = total.ToString();

                }
                else
                {
                    promo = null;
                    discount = 0;
                    labeldisc2.Text = " ";
                    labelDisc.Text = "0";
                    MessageBox.Show("Cannot use your code because its have minimum spend.", "Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPromo.Text = "";
                }
            }
            else
            {
                promo = null;
                discount = 0;
                labeldisc2.Text = " ";
                labelDisc.Text = "0";
                int hargasetelah = Convert.ToInt32(labelSubtotal.Text);
                labeldisc2.Text = "(" + discount.ToString() + "%)";
                int pajak = hargasetelah * 10 / 100;
                int charge = pajak * 5 / 100;
                labelTax.Text = pajak.ToString();
                labelService.Text = charge.ToString();
                int subtotal = Convert.ToInt32(labelSubtotal.Text);
                int total = subtotal + pajak + charge;
                labelTotal.Text = total.ToString();
            }

        }

        private void dgvPayment_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            discount = 0;
            promo = null;
            labeldisc2.Text = " ";
            labelDisc.Text = "0";
            txtPromo.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtPromo.Text == "")
            {
                discount = 0;
                int harga = Convert.ToInt32(labelTotal.Text);
                var paypopup = new PaymentConfirmForm();
                paypopup.table = tableoke.ToString();
                paypopup.customer = customeroke;
                paypopup.total = harga;
                paypopup.ShowDialog();

                if (paypopup.submit == true)
                {
                    var databaru = new payment
                    {
                        header_order_id = data.id,
                        payment_type = paypopup.paymenttype,
                        amount_to_pay = harga,
                        amount_paid = paypopup.pay
                    };
                    db.payments.Add(databaru);
                    db.SaveChanges();
                }
                LoadData();
                dgvDetail.DataSource = null;
                promo = null;
                discount = 0;
                txtPromo.Text = "";
                labelSubtotal.Text = "0";
                labelTax.Text = "0";
                labeldisc2.Text = " ";
                labelDisc.Text = "0";
                labelService.Text = "0";
                labelTotal.Text = "0";
            }
            else
            {
            var sekarang = DateTime.Now;
            var exist = db.promotions
                .FirstOrDefault(x => x.code == txtPromo.Text && x.start_time < sekarang && x.end_time > sekarang);
            if (exist != null)
            {
                decimal minimum = exist.minimum_spent;
                int minimumspend = Convert.ToInt32(minimum);
                if (Convert.ToInt32(labelSubtotal.Text) > minimumspend)
                {
                        int harga = Convert.ToInt32(labelTotal.Text);
                        var paypopup = new PaymentConfirmForm();
                        paypopup.table = tableoke.ToString();
                        paypopup.customer = customeroke;
                        paypopup.total = harga;
                        paypopup.ShowDialog();

                        if (paypopup.submit == true)
                        {
                            var databaru = new payment
                            {
                                header_order_id = data.id,
                                promotion_id = promo.id,
                                payment_type = paypopup.paymenttype,
                                amount_to_pay = harga,
                                amount_paid = paypopup.pay
                            };
                            db.payments.Add(databaru);
                            db.SaveChanges();
                        }
                        LoadData();
                        dgvDetail.DataSource = null;
                        promo = null;
                        discount = 0;
                        txtPromo.Text = "";
                        labelSubtotal.Text = "0";
                        labelTax.Text = "0";
                        labeldisc2.Text = " ";
                        labelDisc.Text = "0";
                        labelService.Text = "0";
                        labelTotal.Text = "0";
                    }
                else
                {
                    MessageBox.Show("You cannot use the promo code because its have minimum spend", "Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                    MessageBox.Show("You cannot use the promo code because its expired", "Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                }

            }
        }
    }
}
