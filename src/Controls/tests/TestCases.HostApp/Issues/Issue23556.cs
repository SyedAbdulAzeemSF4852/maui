using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls.TestCases.HostApp.Issues
{
	[Issue(IssueTracker.Github, 23556, "when 'ScrollToPosition' is set to 'MakeVisible' & 'Start', the item is not fully visible", PlatformAffected.UWP)]
	class Issue23556 : ContentPage
	{
		CollectionView collectionView;
		public List<AnimalGroup> Animals { get; private set; } = new List<AnimalGroup>();
		public Issue23556()
		{
			var grid = new Grid
			{
				RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
			}
			};
			var button = new Button
			{
				AutomationId = "button",
				Text = "Scroll to Proboscis Monkey",
			};
			button.Clicked += (obj, args) =>
			{
				AnimalGroup group = Animals.FirstOrDefault(a => a.Name == "Monkeys");
				Animal monkey = group.FirstOrDefault(m => m.Name == "Proboscis Monkey");
				collectionView.ScrollTo(monkey, group, ScrollToPosition.Start, true);
			};
			grid.Add(button, 0, 0);
			collectionView = new CollectionView
			{
				AutomationId = "collection",
				IsGrouped = true,
				ItemsSource = Animals
			};
			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontAttributes = FontAttributes.Bold,
					HeightRequest = 100,
				};
				label.SetBinding(Label.TextProperty, new Binding("Name"));
				label.SetBinding(Label.AutomationIdProperty, new Binding("Name"));
				return label;
			});
			collectionView.GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
					BackgroundColor = Colors.LightGray
				};
				label.SetBinding(Label.TextProperty, new Binding("Name"));
				return label;
			});
			collectionView.GroupFooterTemplate = new DataTemplate(() =>
			{
				return new Label
				{
					Text = "The END",
					BackgroundColor = Colors.LightGray
				};
			});
			grid.Add(collectionView, 0, 1);
			Content = grid;
			Animals.Add(new AnimalGroup("Bears", new List<Animal>
			{
				new Animal { Name = "Sloth Bear" },
				new Animal { Name = "Sun Bear" },
				new Animal { Name = "Polar Bear" },
				new Animal { Name = "Spectacled Bear" },
			}));
			Animals.Add(new AnimalGroup("Monkeys", new List<Animal>
			{
				new Animal { Name = "Squirrel Monkey" },
				new Animal { Name = "Golden Lion Tamarin" },
				new Animal { Name = "Howler Monkey" },
				new Animal { Name = "Japanese Macaque" },
				new Animal { Name = "Mandrill" },
				new Animal { Name = "Proboscis Monkey" },
				new Animal { Name = "Red-shanked Douc" },
				new Animal { Name = "Gray-shanked Douc" },
				new Animal { Name = "Golden Snub-nosed Monkey" },
				new Animal { Name = "Black Snub-nosed Monkey" },
				new Animal { Name = "Tonkin Snub-nosed Monkey" },
				new Animal { Name = "Thomas's Langur" },
				new Animal { Name = "Purple-faced Langur" },
				new Animal { Name = "Gelada" },
				new Animal { Name = "Blue Monkey" },
			}));
		}
	}
	public class Animal
	{
		public string Name { get; set; }
	}
	public class AnimalGroup : List<Animal>
	{
		public string Name { get; private set; }
		public AnimalGroup(string name, List<Animal> animals) : base(animals)
		{
			Name = name;
		}
	}

}
