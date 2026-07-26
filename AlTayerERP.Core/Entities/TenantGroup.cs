using System;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// المجموعة التجارية (TenantGroup) وهي المستوى الأعلى قبل الشركات.
    /// حقول التدقيق مصدرها Backend ولا تقبل من واجهة المستخدم.
    /// </summary>
    public class TenantGroup
    {
        public string Group_ID { get; set; } = string.Empty;
        public string Group_Code { get