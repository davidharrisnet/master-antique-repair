namespace MasterAntiqueRepair
{
    public static class UiHelpers
    {
        // Maps a ticket's State to a Bootstrap contextual label class - the color itself
        // comes entirely from Bootstrap (label-warning/label-info/label-success), not custom CSS.
        public static string StatusLabelClass(State.RepairState state)
        {
            switch (state)
            {
                case State.RepairState.SUBMITTED:
                    return "label-warning";
                case State.RepairState.INPROGRESS:
                    return "label-info";
                case State.RepairState.COMPLETED:
                    return "label-success";
                default:
                    return "label-default";
            }
        }

        // Truncates long free text for a compact list/grid cell. The full text still goes
        // out to the client (as a popover's data-content) so nothing is actually hidden -
        // this only shortens what's shown inline before the reader clicks to expand it.
        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, maxLength) + "...";
        }
    }
}
