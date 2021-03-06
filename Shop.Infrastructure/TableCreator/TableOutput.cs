﻿using Shop.Infrastructure.TableCreator.Column;

namespace Shop.Infrastructure.TableCreator
{
    using System.Collections.Generic;

    public class TableOutput
    {
        public List<TableColumnDefinition> Columns { get; set; }

        public List<TableRow> Rows { get; set; }

        public int TotalItems { get; set; }
    }
}