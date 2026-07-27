using System.Net;
using System.Text;
using Android.Content;
using Android.Print;
using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Maui.ApplicationModel;
using AndroidWebView = Android.Webkit.WebView;
using AndroidWebViewClient = Android.Webkit.WebViewClient;
using AndroidWebResourceRequest = Android.Webkit.IWebResourceRequest;
using AndroidWebResourceError = Android.Webkit.WebResourceError;

namespace AlTayerERP.Mobile.Office.Platforms.Android;

public sealed class ReceiptVoucherPrintService : IReceiptVoucherPrintService
{
    private static AndroidWebView? _activePrintView;

    public async Task PrintAsync(ReceiptVoucherDetailsDto voucher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(voucher);

        var activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("تعذر الوصول إلى شاشة Android الحالية للطباعة.");

        var html = BuildHtml(voucher);
        var webView = new AndroidWebView(activity);
        webView.Settings.JavaScriptEnabled = false;
        webView.Settings.DefaultTextEncodingName = "utf-8";

        var loaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        webView.SetWebViewClient(new PrintWebViewClient(loaded));
        _activePrintView = webView;

        webView.LoadDataWithBaseURL(null, html, "text/html", "utf-8", null);
        using var registration = cancellationToken.Register(() => loaded.TrySetCanceled(cancellationToken));
        await loaded.Task;

        var printManager = (PrintManager?)activity.GetSystemService(Context.PrintService)
            ?? throw new InvalidOperationException("خدمة الطباعة غير متاحة على الجهاز.");

        var header = voucher.Header;
        var jobName = $"Receipt-{Safe(header.VoucherNo)}";
        var adapter = webView.CreatePrintDocumentAdapter(jobName);
        printManager.Print(jobName, adapter, new PrintAttributes.Builder()
            .SetMediaSize(PrintAttributes.MediaSize.IsoA4)
            .SetColorMode(PrintColorMode.Color)
            .Build());
    }

    private static string BuildHtml(ReceiptVoucherDetailsDto voucher)
    {
        var h = voucher.Header;
        var rows = new StringBuilder();
        foreach (var line in voucher.Details.OrderBy(x => x.LineNo))
        {
            rows.Append("<tr>")
                .Append(Cell(line.LineNo.ToString()))
                .Append(Cell(line.AccountDisplay))
                .Append(Cell(line.CostCenterDisplay ?? "—"))
                .Append(Cell(line.CurrencyDisplay))
                .Append(Cell(line.DebitAmount.ToString("N2")))
                .Append(Cell(line.CreditAmount.ToString("N2")))
                .Append(Cell(line.Description ?? string.Empty))
                .Append("</tr>");
        }

        var watermark = h.IsPosted ? "نسخة رسمية" : "مسودة";
        return $$"""
<!doctype html>
<html lang="ar" dir="rtl">
<head>
<meta charset="utf-8" />
<style>
@page { size: A4; margin: 12mm; }
body { font-family: Arial, Tahoma, sans-serif; color:#17324D; direction:rtl; margin:0; }
.header { border:2px solid #17324D; padding:14px; text-align:center; border-radius:10px; }
.brand { font-size:22px; font-weight:bold; }
.title { font-size:26px; font-weight:bold; margin-top:6px; }
.meta { width:100%; margin-top:14px; border-collapse:collapse; }
.meta td { border:1px solid #AAB7C4; padding:8px; vertical-align:top; }
.label { font-weight:bold; color:#0B6B87; }
.amount { font-size:20px; font-weight:bold; }
table.lines { width:100%; border-collapse:collapse; margin-top:16px; font-size:11px; }
.lines th,.lines td { border:1px solid #8FA1B3; padding:6px; text-align:right; }
.lines th { background:#EAF2F8; }
.footer { margin-top:28px; display:flex; justify-content:space-between; gap:20px; }
.sign { width:30%; text-align:center; border-top:1px solid #17324D; padding-top:8px; }
.watermark { position:fixed; top:45%; left:10%; right:10%; text-align:center; font-size:72px; color:rgba(180,35,24,.08); transform:rotate(-25deg); z-index:-1; }
.small { font-size:10px; color:#667788; margin-top:14px; }
</style>
</head>
<body>
<div class="watermark">{{E(watermark)}}</div>
<div class="header">
  <div class="brand">الطائر للبرمجيات</div>
  <div class="title">سند قبض</div>
</div>
<table class="meta">
<tr><td><span class="label">رقم السند:</span> {{E(h.VoucherNo)}}</td><td><span class="label">التاريخ:</span> {{h.VoucherDate:yyyy/MM/dd}}</td></tr>
<tr><td colspan="2"><span class="label">استلمنا من:</span> {{E(h.ReceivedFromName ?? "—")}}</td></tr>
<tr><td><span class="label">الصندوق/البنك:</span> {{E(h.CashAccountDisplay)}}</td><td><span class="label">العملة:</span> {{E(h.CurrencyDisplay)}}</td></tr>
<tr><td><span class="label">المرجع:</span> {{E(h.ReferenceNo ?? "—")}}</td><td><span class="label">الحالة:</span> {{E(h.WorkflowStatus)}}</td></tr>
<tr><td colspan="2"><span class="label">البيان:</span> {{E(h.Description ?? "—")}}</td></tr>
<tr><td colspan="2" class="amount"><span class="label">الإجمالي المحلي:</span> {{h.LocalTotal:N2}}</td></tr>
</table>
<table class="lines">
<thead><tr><th>#</th><th>الحساب</th><th>مركز التكلفة</th><th>العملة</th><th>مدين</th><th>دائن</th><th>البيان</th></tr></thead>
<tbody>{{rows}}</tbody>
</table>
<div class="footer"><div class="sign">المستلم</div><div class="sign">المحاسب</div><div class="sign">المعتمد</div></div>
<div class="small">تم إنشاء هذه النسخة من تطبيق المكتب — عدد مرات الطباعة المسجل قبل هذه العملية: {{h.PrintCount}}</div>
</body>
</html>
""";
    }

    private static string Cell(string value) => $"<td>{E(value)}</td>";
    private static string E(string value) => WebUtility.HtmlEncode(value);
    private static string Safe(string value) => string.Concat(value.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_'));

    private sealed class PrintWebViewClient(TaskCompletionSource loaded) : AndroidWebViewClient
    {
        public override void OnPageFinished(AndroidWebView? view, string? url)
        {
            base.OnPageFinished(view, url);
            loaded.TrySetResult();
        }

        public override void OnReceivedError(AndroidWebView? view, AndroidWebResourceRequest? request, AndroidWebResourceError? error)
        {
            base.OnReceivedError(view, request, error);
            loaded.TrySetException(new InvalidOperationException("تعذر تجهيز معاينة سند القبض للطباعة."));
        }
    }
}
