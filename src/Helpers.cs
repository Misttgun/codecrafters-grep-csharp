namespace codecrafters_grep;

public static class Helpers
{
    public static int GetClosingParenthesisIndex(string input)
    {
        int index = -1;
        Stack<char> stack = new Stack<char>();
        for (int i = 0; i < input.Length; i++)
        {
            var symbol = input[i];
            switch (symbol)
            {
                case '(':
                    stack.Push(symbol);
                    break;
                case ')' when stack.Count == 0:
                    return -1;
                case ')':
                    stack.Pop();
                    index = i;
                    if (stack.Count == 0)
                        return index;
                    break;
            }
        }
        
        return index;
    }
}