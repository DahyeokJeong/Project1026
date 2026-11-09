using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class GuildCreationUI : MonoBehaviour
{
    [SerializeField] private Font koreanFont = null;

    public event Action<GuildCreationForm.Draft> CreationRequested;

    private GuildCreationForm form;

    private void OnEnable()
    {
        form = new GuildCreationForm(koreanFont, draft => CreationRequested?.Invoke(draft));
        GetComponent<UIDocument>().rootVisualElement.Add(form);
    }

    private void OnDisable()
    {
        form?.RemoveFromHierarchy();
    }
}

public sealed class GuildCreationForm : ScrollView
{
    public readonly struct Draft
    {
        public readonly string Name;
        public readonly string Restriction;
        public readonly bool AllowInstantJoin;
        public readonly string Introduction;
        public readonly string BannerId;

        public Draft(string name, string restriction, bool allowInstantJoin, string introduction, string bannerId)
        {
            Name = name;
            Restriction = restriction;
            AllowInstantJoin = allowInstantJoin;
            Introduction = introduction;
            BannerId = bannerId;
        }
    }

    private static readonly Color Ink = new(0.89f, 0.91f, 0.94f);
    private static readonly Color Muted = new(0.65f, 0.71f, 0.78f);
    private static readonly Color Gold = new(0.95f, 0.77f, 0.45f);
    private static readonly Color Line = new(0.27f, 0.33f, 0.42f);
    private static readonly Color Well = new(0.08f, 0.11f, 0.16f);

    private static readonly (string Id, string Name, Color Color)[] Banners =
    {
        ("dawn", "여명의 별", new Color(0.53f, 0.21f, 0.26f)),
        ("tide", "푸른 산맥", new Color(0.15f, 0.37f, 0.59f)),
        ("grove", "숲의 수호자", new Color(0.20f, 0.44f, 0.35f)),
        ("moon", "은빛 달", new Color(0.37f, 0.27f, 0.56f)),
        ("flame", "잿불", new Color(0.58f, 0.32f, 0.16f)),
        ("crown", "검은 왕관", new Color(0.27f, 0.30f, 0.37f))
    };

    private int selected;
    private readonly Button[] choices = new Button[Banners.Length];

    public GuildCreationForm(Font font, Action<Draft> submit) : base(ScrollViewMode.VerticalAndHorizontal)
    {
        style.flexGrow = 1;
        style.backgroundColor = new Color(0.055f, 0.07f, 0.10f);
        style.color = Ink;
        style.fontSize = 14;
        if (font != null)
            style.unityFont = font;

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.minWidth = 760;
        layout.style.maxWidth = 1100;
        layout.style.width = Length.Percent(100);
        layout.style.alignSelf = Align.Center;
        Pad(layout, 20);
        Add(layout);

        var left = Panel(layout, "01   길드 창설", "길드의 기본 정보와 가입 방식을 설정하세요.");
        var right = Panel(layout, "02   배너 디자인", "아래 목록에서 길드를 대표할 문장을 고르세요.");
        right.style.marginLeft = 16;

        var settings = new VisualElement();
        settings.style.flexDirection = FlexDirection.Row;
        left.Add(settings);

        var badge = Banner(74, 88, () => selected);
        var edit = new Button(() => choices[selected].Focus()) { text = "배너 선택" };
        edit.tooltip = "배너 디자인 선택";
        edit.style.marginRight = 14;
        edit.Add(badge);
        settings.Add(edit);

        var rules = new VisualElement();
        rules.style.flexGrow = 1;
        settings.Add(rules);
        var restriction = Field(rules, "가입 제한", 40);
        Caption(rules, "즉시 가입");
        var join = new RadioButtonGroup(null, new List<string> { "허용", "허용 안 함" });
        join.value = 0;
        join.Q(className: RadioButtonGroup.containerUssClassName).style.flexDirection = FlexDirection.Row;
        rules.Add(join);

        var guildName = Field(left, "길드 이름", 24);
        var introduction = Field(left, "길드 소개", 300, true);
        var status = Caption(left, " ");
        var create = new Button(() =>
        {
            string name = guildName.value.Trim();
            if (name.Length == 0)
            {
                status.text = "길드 이름을 입력하세요.";
                guildName.Focus();
                return;
            }

            status.text = " ";
            submit(new Draft(name, restriction.value.Trim(), join.value == 0,
                introduction.value.Trim(), Banners[selected].Id));
        }) { text = "길드 창설" };
        create.style.alignSelf = Align.FlexEnd;
        create.style.width = 122;
        create.style.height = 40;
        create.style.backgroundColor = Gold;
        create.style.color = Well;
        create.style.unityFontStyleAndWeight = FontStyle.Bold;
        left.Add(create);

        var preview = Banner(132, 156, () => selected);
        preview.style.alignSelf = Align.Center;
        right.Add(preview);
        var bannerName = Caption(right, Banners[0].Name);
        bannerName.style.color = Gold;
        bannerName.style.unityTextAlign = TextAnchor.MiddleCenter;

        Caption(right, "디자인 목록");
        var grid = new VisualElement();
        grid.style.flexDirection = FlexDirection.Row;
        grid.style.flexWrap = Wrap.Wrap;
        grid.style.backgroundColor = Well;
        Frame(grid, Line);
        Pad(grid, 6);
        right.Add(grid);

        for (int i = 0; i < Banners.Length; i++)
        {
            int index = i;
            var option = new Button(() =>
            {
                Frame(choices[selected], Line);
                selected = index;
                Frame(choices[selected], Gold);
                bannerName.text = Banners[selected].Name;
                preview.MarkDirtyRepaint();
                badge.MarkDirtyRepaint();
            });
            option.style.width = Length.Percent(30);
            option.style.flexGrow = 1;
            option.style.color = Ink;
            option.style.flexDirection = FlexDirection.Column;
            option.style.alignItems = Align.Center;
            option.style.marginTop = 4;
            option.style.marginBottom = 4;
            option.style.backgroundColor = new Color(0.14f, 0.18f, 0.24f);
            Frame(option, index == selected ? Gold : Line);
            option.Add(Banner(38, 46, () => index));
            option.Add(new Label(Banners[index].Name) { pickingMode = PickingMode.Ignore });
            choices[index] = option;
            grid.Add(option);
        }
    }

    private static VisualElement Panel(VisualElement parent, string title, string description)
    {
        var panel = new VisualElement();
        panel.style.flexBasis = 0;
        panel.style.flexGrow = 1;
        panel.style.backgroundColor = new Color(0.12f, 0.15f, 0.20f);
        Frame(panel, Line);
        Pad(panel, 20);
        parent.Add(panel);

        var heading = new Label(title);
        heading.style.fontSize = 23;
        heading.style.color = Gold;
        heading.style.unityFontStyleAndWeight = FontStyle.Bold;
        panel.Add(heading);

        var subtitle = Caption(panel, description);
        subtitle.style.marginTop = 5;
        subtitle.style.marginBottom = 16;
        return panel;
    }

    private static Label Caption(VisualElement parent, string text)
    {
        var label = new Label(text);
        label.style.color = Muted;
        label.style.marginTop = 9;
        label.style.marginBottom = 4;
        label.style.whiteSpace = WhiteSpace.Normal;
        parent.Add(label);
        return label;
    }

    private static TextField Field(VisualElement parent, string label, int maxLength, bool multiline = false)
    {
        Caption(parent, label);
        var field = new TextField { maxLength = maxLength, multiline = multiline, tooltip = label };
        field.style.height = multiline ? 155 : 32;
        field.style.whiteSpace = multiline ? WhiteSpace.Normal : WhiteSpace.NoWrap;
        parent.Add(field);
        var input = field.Q(className: TextField.inputUssClassName);
        input.style.backgroundColor = Well;
        input.style.color = Ink;
        Frame(input, Line);
        return field;
    }

    private static void Pad(VisualElement element, float padding)
    {
        element.style.paddingTop = element.style.paddingBottom = padding;
        element.style.paddingLeft = element.style.paddingRight = padding;
    }

    private static void Frame(VisualElement element, Color color)
    {
        var style = element.style;
        style.borderLeftColor = style.borderRightColor = style.borderTopColor = style.borderBottomColor = color;
        style.borderLeftWidth = style.borderRightWidth = style.borderTopWidth = style.borderBottomWidth = 1;
        style.borderTopLeftRadius = style.borderTopRightRadius =
            style.borderBottomLeftRadius = style.borderBottomRightRadius = 6;
    }

    private static VisualElement Banner(float width, float height, Func<int> getIndex)
    {
        var element = new VisualElement { pickingMode = PickingMode.Ignore };
        element.style.width = width;
        element.style.height = height;
        element.generateVisualContent += context =>
        {
            int index = getIndex();
            var painter = context.painter2D;
            var size = element.contentRect.size;
            painter.fillColor = Banners[index].Color;
            painter.strokeColor = Gold;
            painter.lineWidth = 2;
            Polygon(painter, size, new(0.12f, 0.07f), new(0.88f, 0.07f),
                new(0.88f, 0.73f), new(0.50f, 0.94f), new(0.12f, 0.73f));
            painter.Stroke();

            painter.fillColor = Gold;
            switch (index)
            {
                case 0:
                    Polygon(painter, size, new(0.50f, 0.22f), new(0.56f, 0.38f), new(0.73f, 0.43f),
                        new(0.56f, 0.48f), new(0.50f, 0.64f), new(0.44f, 0.48f),
                        new(0.27f, 0.43f), new(0.44f, 0.38f));
                    break;
                case 1:
                    Polygon(painter, size, new(0.24f, 0.54f), new(0.44f, 0.34f), new(0.54f, 0.46f),
                        new(0.67f, 0.32f), new(0.76f, 0.54f));
                    break;
                case 2:
                    Polygon(painter, size, new(0.50f, 0.22f), new(0.72f, 0.52f), new(0.56f, 0.52f),
                        new(0.56f, 0.66f), new(0.44f, 0.66f), new(0.44f, 0.52f), new(0.28f, 0.52f));
                    break;
                case 3:
                    painter.BeginPath();
                    painter.Arc(Vector2.Scale(size, new(0.50f, 0.43f)), size.x * 0.21f, 0, 360);
                    painter.ClosePath();
                    painter.Fill();
                    painter.fillColor = Banners[index].Color;
                    painter.BeginPath();
                    painter.Arc(Vector2.Scale(size, new(0.59f, 0.37f)), size.x * 0.17f, 0, 360);
                    painter.ClosePath();
                    painter.Fill();
                    break;
                case 4:
                    Polygon(painter, size, new(0.52f, 0.22f), new(0.71f, 0.51f), new(0.61f, 0.65f),
                        new(0.39f, 0.65f), new(0.28f, 0.52f), new(0.40f, 0.36f), new(0.46f, 0.47f));
                    break;
                case 5:
                    Polygon(painter, size, new(0.27f, 0.31f), new(0.41f, 0.43f), new(0.50f, 0.27f),
                        new(0.59f, 0.43f), new(0.73f, 0.31f), new(0.65f, 0.58f), new(0.35f, 0.58f));
                    break;
            }
        };
        return element;
    }

    private static void Polygon(Painter2D painter, Vector2 size, params Vector2[] points)
    {
        painter.BeginPath();
        painter.MoveTo(Vector2.Scale(size, points[0]));
        for (int i = 1; i < points.Length; i++)
            painter.LineTo(Vector2.Scale(size, points[i]));
        painter.ClosePath();
        painter.Fill();
    }
}
