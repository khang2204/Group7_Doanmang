using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Diagnostics;
using System.Net.Http.Headers;
namespace HTTPClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        void ShowHeaders(HttpResponseHeaders headers)
        {
            if (headers == null)
            {
                // Headers is null, handle this case if needed
                return;
            }

            // Clear existing items in listView1
            listView1.Items.Clear();

            int numSTT = 0;
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                ListViewItem headerList = new ListViewItem();
                headerList.Text = numSTT.ToString();
                headerList.SubItems.Add(header.Key);

                // Check if the value is null
                if (header.Value != null)
                {
                    // Process the header values
                    List<string> values = header.Value.ToList();

                    if (values.Any())
                    {
                        // Join values into a single string
                        headerList.SubItems.Add(string.Join(",", values));
                    }
                    else
                    {
                        // If the value list is empty, just add an empty string
                        headerList.SubItems.Add("");
                    }
                }
                else
                {
                    // If the value is null, add an empty string
                    headerList.SubItems.Add("");
                }

                numSTT++;
                listView1.Items.Add(headerList);
            }
        }

        private async Task<string> GetHTMLAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("Content-Type", "application/");

                HttpResponseMessage response = await client.GetAsync(url);

                ShowHeaders(response.Headers);
              
                if (response.IsSuccessStatusCode)
                {
                    // Đọc nội dung với Encoding.UTF8 để đảm bảo đọc đúng bộ mã.
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                else
                {
                    return "Error: " + response.StatusCode;
                }
            }
        }

        private async Task<string> GetDataWithCookieAsync(string url, string cookie)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                // Thêm tiêu đề User-Agent vào yêu cầu để định danh trình duyệt khi gửi yêu cầu.
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");

                // Phân tách cookie thành các cặp key-value và thêm chúng vào CookieContainer.
                var cookiePairs = cookie.Split(';');
                foreach (var pair in cookiePairs)
                {
                    var cookieKeyValue = pair.Split('=');
                    if (cookieKeyValue.Length == 2)
                    {
                        request.Headers.Add("Cookie", $"{cookieKeyValue[0].Trim()}={cookieKeyValue[1].Trim()}");
                    }
                }

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                else
                {
                    return "Error: " + response.StatusCode;
                }
            }
        }


        private async Task<string> PostDataAsync(string szUrl, string postData)
        {
            using (HttpClient client = new HttpClient())
            {
                // Tạo nội dung POST từ dữ liệu đã cung cấp
                HttpContent content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                // Gửi yêu cầu POST đến URL đã chỉ định và nhận phản hồi
                HttpResponseMessage response = await client.PostAsync(szUrl, content);

                // Đảm bảo yêu cầu thành công
                if (response.IsSuccessStatusCode)
                {
                    // Đọc nội dung phản hồi và trả về dưới dạng chuỗi
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                else
                {
                    // Xử lý trường hợp lỗi nếu cần thiết
                    return "Error: " + response.StatusCode;
                }
            }
        }
        private async Task<string> DeleteDataAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.DeleteAsync(url);


                if (response.IsSuccessStatusCode)
                {
                    string noti = "DELETE request successful.";
                    return noti;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
        }
        static async Task<string> PutDataAsync(string url, string data)
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc nội dung phản hồi và trả về dưới dạng chuỗi
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
        }
        
        private async void button1_Click(object sender, EventArgs e)
        {
            switch (comboBox1.Text)
            {
                case "GET":
                {
                    if (cookieBox.Text == "")
                    {
                        string responseData = await GetHTMLAsync(textBox1.Text);
                        richTextBox1.Text = responseData;
                    }
                    else
                    {
                        string responseData = await GetDataWithCookieAsync(textBox1.Text, cookieBox.Text);
                        richTextBox1 .Text = responseData;
                    }
                    break;
                }
                    
                case "POST":
                {
                    string responseData = await PostDataAsync(textBox1.Text, postData.Text);
                    richTextBox1.Text = responseData;
                    break;
                }
                case "DELETE":
                {
                    string noti = await DeleteDataAsync(textBox1.Text);
                    richTextBox1.Text = noti;
                    break;
                }
                case "PUT":
                {
                    string noti = await PutDataAsync(textBox1.Text, postData.Text);
                    richTextBox1.Text = noti;
                    break;
                }
                default:
                {
                    MessageBox.Show("Invalid method");
                    break;
                }
            }
            File.WriteAllText("res.html", richTextBox1.Text);
            Process.Start("res.html");
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
