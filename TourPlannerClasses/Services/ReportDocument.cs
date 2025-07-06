using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TourPlannerClasses.Models;

namespace TourPlanner.BusinessLogic.Services
{
    public class TourReportDocument : IDocument
    {
        private readonly Tours _tour;

        public TourReportDocument(Tours tour)
        {
            _tour = tour;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Compose starting...");

                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Text($"Tour Report - {_tour.Name}")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"From: {_tour.From} ({_tour.FromLat},{_tour.FromLng})");
                        col.Item().Text($"To: {_tour.To} ({_tour.ToLat},{_tour.ToLng})");
                        col.Item().Text($"Description: {_tour.Description}");
                        col.Item().Text($"Transport: {_tour.Transport}");
                        col.Item().Text($"Distance: {_tour.Distance} km");
                        col.Item().Text($"Duration: {_tour.Duration.ToString(@"hh\:mm\:ss")}");
                        col.Item().Text($"Popularity: {_tour.Popularity} logs");
                        col.Item().Text($"Child Friendly: {(_tour.IsChildFriendly ? "Yes" : "No")}");

                        col.Item().Text("Tour Logs:").Bold();

                        col.Item().Text("Map:").Bold();
                        col.Item().Image("MyMap.png", ImageScaling.FitWidth);

                        if (_tour.Tourlogs == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Tourlogs IS NULL!");
                            col.Item().Text("No tour logs available.");
                        }
                        else if (!_tour.Tourlogs.Any())
                        {
                            col.Item().Text("No tour logs available.");
                        }
                        else
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(50);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Date").Bold();
                                    header.Cell().Text("Comment").Bold();
                                    header.Cell().Text("Difficulty").Bold();
                                    header.Cell().Text("Distance").Bold();
                                    header.Cell().Text("Rating").Bold();
                                });

                                foreach (var log in _tour.Tourlogs)
                                {
                                    if (log == null)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Found null Tourlog!");
                                        continue;
                                    }

                                    table.Cell().Text(log.Date.ToShortDateString());
                                    table.Cell().Text(log.Comment ?? "");
                                    table.Cell().Text(log.Difficulty.ToString());
                                    table.Cell().Text($"{log.TotalDistance} km");
                                    table.Cell().Text(log.Rating.ToString());
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text($"Generated on {DateTime.Now}");
                });

                System.Diagnostics.Debug.WriteLine("Compose finished.");
            }
            catch(TourReportGenerationException trex)
            {
                MessageBox.Show("Error creating PDF:\n" + trex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }

    public class TourSummaryReportDocument : IDocument
    {
        private readonly List<TourSummaryDto> _summaries;

        public TourSummaryReportDocument(List<TourSummaryDto> summaries)
        {
            _summaries = summaries;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            try
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Tour Summary Report")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.ConstantColumn(100);
                            c.ConstantColumn(100);
                            c.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Tour Name").Bold();
                            header.Cell().AlignRight().Text("Avg Distance (km)").Bold();
                            header.Cell().AlignRight().Text("Avg Duration").Bold();
                            header.Cell().AlignRight().Text("Avg Rating").Bold();
                        });

                        foreach (var summary in _summaries)
                        {
                            table.Cell().Text(summary.TourName);
                            table.Cell().AlignRight().Text(summary.AverageDistance.ToString("F2"));
                            table.Cell().AlignRight().Text(summary.AverageDuration.ToString(@"hh\:mm\:ss"));
                            table.Cell().AlignRight().Text(summary.AverageRating.ToString("F1"));
                        }
                    });

                    page.Footer().AlignCenter().Text($"Generated on {DateTime.Now}");
                });
            }
            catch(TourReportGenerationException trex)
            {
                MessageBox.Show("Error generating summary report:\n" + trex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
            }   
        }
    }
    public class TourSummaryDto
    {
        public string TourName { get; set; }
        public double AverageDistance { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public double AverageRating { get; set; }
    }
}
