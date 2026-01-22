using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using bth_dc_inventory.Models;

public class InvoiceDocument : IDocument
{
    public List<Item> Items { get; }

    public InvoiceDocument(List<Item> items)
    {
        Items = items;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(20);

            page.Header()
                .Text("Laporan Inventaris")
                .SemiBold()
                .FontSize(16);

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Item").SemiBold();
                    header.Cell().Text("Category").SemiBold();
                    header.Cell().Text("Qty").SemiBold();
                    header.Cell().Text("Price").SemiBold();
                });

                foreach (var item in Items)
                {
                    table.Cell().Text(item.ItemName ?? "-");
                    table.Cell().Text(item.Category?.CategoryName ?? "-");
                    table.Cell().Text(item.Quantity.ToString());
                    table.Cell().Text(item.BuyingPrice.ToString("C"));
                }
            });

            page.Footer()
                .AlignCenter()
                .Text(DateTime.Now.ToString("dd MMM yyyy HH:mm"));
        });
    }
}