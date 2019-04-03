using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class Recordset
    {
        public string account_id { get; set; }
        public string seq_id { get; set; }
        public string bill_id { get; set; }
        public string file_type { get; set; }
        public string status { get; set; }
        public string profile_type { get; set; }
        public string bill_date { get; set; }
        public string from_date { get; set; }
        public string to_date { get; set; }
        public string current_cycle_charges_with_tax { get; set; }
        public string __invalid_name__total_charges { get; set; }
        public string due_date { get; set; }
        public string clear_status { get; set; }
        public string is_paid { get; set; }
        public string pending_invoice_amount { get; set; }
    }

    public class InvoiceDetails
    {
        public List<Recordset> recordset { get; set; }
        public int totalrecordcnt { get; set; }
        public int recordcnt { get; set; }
    }

    public class BSSInvoiceResponse
    {
        public string request_id { get; set; }
        public string request_timestamp { get; set; }
        public string response_timestamp { get; set; }
        public string action { get; set; }
        public string response_id { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string source_node { get; set; }
        public string result_code { get; set; }
        public string result_desc { get; set; }
        public InvoiceDetails invoice_details { get; set; }
        public SetParam dataset { get; set; }
    }

    public class BSSInvoiceResponseObject
    {
        public BSSInvoiceResponse Response { get; set; }
    }
}
