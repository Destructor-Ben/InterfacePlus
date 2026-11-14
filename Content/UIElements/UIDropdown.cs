using System.Numerics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace InterfacePlus.Content.UIElements;

public class UIDropdown<T> : UIPanel where T : IEqualityOperators<T, T, bool>
{
	public record struct Option(string Name, T Value);

	public T CurrentValue { get; private set; }
	public T CurrentHoveredValue { get; private set; }
	public bool HoverValueIsValid { get; private set; } // Fuck C# for not letting me make the above property nullable

	private readonly List<Option> options;
	private readonly List<GroupOptionButton<T>> optionButtons;

	public event Action OnClickOption;

	public UIDropdown(List<Option> options, T currentValue, Vector2 dropdownOrigin = default, float dropdownHAlign = 0f, int buttonWidth = 100, int buttonHeight = 26)
	{
		CurrentValue = currentValue;
		this.options = options;
		optionButtons = [];

		BackgroundColor = new Color(35, 40, 83) * 0.5f;
		BorderColor = new Color(35, 40, 83) * 0.5f;
		IgnoresMouseInteraction = false;
		SetPadding(0f);
		BuildGrid(dropdownOrigin, dropdownHAlign, buttonWidth, buttonHeight);
	}

	private void BuildGrid(Vector2 dropdownOrigin, float dropdownHAlign, int buttonWidth, int buttonHeight)
	{
		const int PanelPadding = 5;
		const int ButtonPadding = 2;

		int finalButtonWidth = buttonWidth;
		int finalButtonHeight = buttonHeight + ButtonPadding;

		var dropdownPanel = new UIPanel {
			Width = new StyleDimension(PanelPadding * 2 + finalButtonWidth, 0f),
			Height = new StyleDimension(options.Count * finalButtonHeight + 5 + 3, 0f), // Weird alignment but ok
			Left = new StyleDimension(dropdownOrigin.X, 0f),
			Top = new StyleDimension(dropdownOrigin.Y, 0f),
			BorderColor = new Color(89, 116, 213, 255) * 0.9f,
			BackgroundColor = new Color(73, 94, 171) * 0.9f,
		};

		dropdownPanel.Left.Pixels -= dropdownHAlign * dropdownPanel.Width.Pixels;
		dropdownPanel.SetPadding(0f);
		Append(dropdownPanel);

		for (int i = 0; i < options.Count; i++) {
			int top = PanelPadding + finalButtonHeight * i;
			int left = PanelPadding;
			var option = options[i];

			var groupOptionButton = new GroupOptionButton<T>(option.Value, null, null, Color.White, null) {
				Width = new StyleDimension(finalButtonWidth, 0f),
				Height = new StyleDimension(finalButtonHeight - ButtonPadding, 0f),
				Top = new StyleDimension(top, 0f),
				Left = new StyleDimension(left, 0f),
				ShowHighlightWhenSelected = false,
			};

			groupOptionButton.SetTextWithoutLocalization(option.Name, 0.8f, Color.White, 0.5f, 0f);
			groupOptionButton.OnLeftClick += (_, _) => ClickOption(option.Value);

			groupOptionButton.OnMouseOver += (_, _) =>
			{
				CurrentHoveredValue = groupOptionButton.OptionValue;
				HoverValueIsValid = true;
			};

			groupOptionButton.OnMouseOut += (_, _) =>
			{
				CurrentHoveredValue = default;
				HoverValueIsValid = false;
			};

			dropdownPanel.Append(groupOptionButton);
			optionButtons.Add(groupOptionButton);
		}

		RefreshSelectionStates();
	}

	private void ClickOption(T value)
	{
		SetCurrentValue(value);
		OnClickOption?.Invoke();
	}

	public void SetCurrentValue(T value)
	{
		CurrentValue = value;
		RefreshSelectionStates();
	}

	private void RefreshSelectionStates()
	{
		foreach (var button in optionButtons) {
			button.SetCurrentOption(CurrentValue);

			if (button.IsSelected)
				button.SetColor(new Color(152, 175, 235), 1f);
			else
				button.SetColor(Colors.InventoryDefaultColor, 0.7f);
		}
	}
}
