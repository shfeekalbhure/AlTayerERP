using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>مدير مرفقات السند عبر API فقط؛ الملفات محفوظة خارج قاعدة البيانات.</summary>
public sealed class FrmVoucherAttachments : BaseForm
{
    private readonly long _voucherId;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false };
    private readonly TextBox _reason = new() { Width = 260, PlaceholderText = "سبب الحذف المنطقي" };
    private List<Row> _items = new();

    public FrmVoucherAttachments(long voucherId)
    {
        _voucherId = voucherId; Text = $"مرفقات السند #{voucherId}"; ApplyBaseFormStyle(); RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8,5,8,5) };
        bar.Controls.AddRange(new Control[] { Button("إضافة ملف", async (_,_) => await UploadAsync()), Button("تنزيل", async (_,_) => await DownloadAsync()), Button("حذف منطقي", async (_,_) => await DeleteAsync()), _reason, Button("تحديث", async (_,_) => await RefreshAsync()) });
        Controls.Add(_grid); Controls.Add(bar); Load += async (_,_) => await RefreshAsync();
    }
    private static Button Button(string text, EventHandler handler){var b=new Button{Text=text,AutoSize=true};b.Click+=handler;return b;}
    private async Task RefreshAsync(){try{_items=await ApiService.Client.GetFromJsonAsync<List<Row>>($"vouchers/{_voucherId}/attachments")??new();_grid.DataSource=_items.Where(x=>x.Active).Select(x=>new {x.Id,الاسم=x.Name,الحجم=x.Bytes,تاريخ_الرفع=x.UploadedAtUtc,الملاحظات=x.Notes}).ToList();}catch(Exception ex){MessageBox.Show(ex.Message,"المرفقات",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private string? Selected()=>_grid.CurrentRow?.Cells["Id"].Value?.ToString();
    private async Task UploadAsync()
    {
        using var dialog=new OpenFileDialog{Filter="مستندات وصور|*.pdf;*.png;*.jpg;*.jpeg;*.xlsx;*.docx"};
        if(dialog.ShowDialog(this)!=DialogResult.OK)return;
        using var stream=File.OpenRead(dialog.FileName);using var form=new MultipartFormDataContent();var content=new StreamContent(stream);content.Headers.ContentType=new MediaTypeHeaderValue("application/octet-stream");form.Add(content,"file",Path.GetFileName(dialog.FileName));
        var response=await ApiService.Client.PostAsync($"vouchers/{_voucherId}/attachments",form);if(!response.IsSuccessStatusCode)MessageBox.Show(await response.Content.ReadAsStringAsync(),"المرفقات");await RefreshAsync();
    }
    private async Task DownloadAsync(){var id=Selected();if(id==null)return;var response=await ApiService.Client.GetAsync($"vouchers/{_voucherId}/attachments/{id}/download");if(!response.IsSuccessStatusCode){MessageBox.Show(await response.Content.ReadAsStringAsync());return;}using var dlg=new SaveFileDialog{FileName=_items.FirstOrDefault(x=>x.Id==id)?.Name??"attachment"};if(dlg.ShowDialog(this)!=DialogResult.OK)return;await File.WriteAllBytesAsync(dlg.FileName,await response.Content.ReadAsByteArrayAsync());}
    private async Task DeleteAsync(){var id=Selected();if(id==null)return;if(string.IsNullOrWhiteSpace(_reason.Text)){MessageBox.Show("سبب الحذف المنطقي مطلوب.");return;}var request=new HttpRequestMessage(HttpMethod.Delete,$"vouchers/{_voucherId}/attachments/{id}"){Content=JsonContent.Create(new {Reason=_reason.Text.Trim()})};var response=await ApiService.Client.SendAsync(request);if(!response.IsSuccessStatusCode)MessageBox.Show(await response.Content.ReadAsStringAsync());else await RefreshAsync();}
    private sealed class Row{public string Id{get;set;}="";public string Name{get;set;}="";public long Bytes{get;set;}public string? Notes{get;set;}public DateTime UploadedAtUtc{get;set;}public bool Active{get;set;}}
}