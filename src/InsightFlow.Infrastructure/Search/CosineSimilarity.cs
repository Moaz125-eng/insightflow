namespace InsightFlow.Infrastructure.Search;

public static class CosineSimilarity
{
    public static float Compute(float[] left, float[] right)
    {
        if (left.Length == 0 || right.Length == 0)
            return 0f;

        var length = Math.Min(left.Length, right.Length);
        double dot = 0;
        double leftMag = 0;
        double rightMag = 0;

        for (var i = 0; i < length; i++)
        {
            dot += left[i] * right[i];
            leftMag += left[i] * left[i];
            rightMag += right[i] * right[i];
        }

        if (leftMag <= 0 || rightMag <= 0)
            return 0f;

        return (float)(dot / (Math.Sqrt(leftMag) * Math.Sqrt(rightMag)));
    }
}
