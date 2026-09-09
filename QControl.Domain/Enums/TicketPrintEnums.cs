namespace QControl.Domain.Enums;

public enum TicketPrintElementType
{
    BranchLogo = 1,
    BranchName = 2,
    LeafServiceName = 3,
    ServiceFullTree = 4,
    TicketNumber = 5,
    SegmentName = 6,
    EstimatedWaitingDuration = 7,
    CustomInputsOrField = 8
}

public enum TicketFontWeight
{
    Normal = 1,
    Bold = 2
}

public enum TicketTextAlign
{
    Left = 1,
    Center = 2,
    Right = 3
}

public enum TicketPrintLanguage
{
    Ar = 1,
    En = 2
}

public enum TicketPrintOverflowBehavior
{
    TrimWithEllipsis = 1
}
