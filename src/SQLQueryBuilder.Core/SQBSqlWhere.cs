using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Core
{
    public class SQBSqlWhere
    {
        public string ColumnName { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }
        public bool ValueHasSingleQuote { get; set; }
        public string FunctionTemplate { get; set; } = "{0}"; // Varsayılan: Fonksiyon yok, sadece kolon adı

        // ValueHasSingleQuote yerine bu iki alan kullanılacak
        public bool IsValueEscaped { get; set; } // Değerin tırnak işaretleri zaten eklenmiş mi?
        public bool IsValueFunction { get; set; } // Name alanı bir fonksiyon mu (örn: LEN(Name))?
    }
}
