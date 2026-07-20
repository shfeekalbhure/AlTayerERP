using System;
using System.Collections.Generic;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// يحتفظ ببيانات جلسة المستخدم الحالية داخل تطبيق سطح المكتب.
    /// </summary>
    public static class CurrentSession
    {
        public static string Company_ID { get; set; } = string.Empty;
        public static string Company_Name { get; set; } = string.Empty;

        public static int Branch_ID { get; set; }
        public static string Branch