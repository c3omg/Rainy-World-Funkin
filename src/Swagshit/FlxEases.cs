using System;
using System.Security.Permissions;
using UnityEngine;

internal static class FlxEase
{
    private static float PI2 = (float)Math.PI / 2f;

	private static float EL = 2 * (float)Math.PI / .45f;
	private static float B1 = 1 / 2.75f;
	private static float B2 = 2 / 2.75f;
	private static float B3 = 1.5f / 2.75f;
	private static float B4 = 2.5f / 2.75f;
	private static float B5 = 2.2f / 2.75f;
	private static float B6 = 2.625f / 2.75f;
	private static float ELASTIC_AMPLITUDE = 1;
	private static float ELASTIC_PERIOD = 0.4f;

	/** @since 4.3.0 */
	public static float linear(float t)
	{
		return t;
	}

	public static float quadIn(float t)
	{
		return t * t;
	}

	public static float quadOut(float t)
	{
		return -t * (t - 2);
	}

	public static float quadInOut(float t)
	{
		return t <= .5 ? t * t * 2 : 1 - (--t) * t * 2;
	}

	public static float cubeIn(float t)
	{
		return t * t * t;
	}

	public static float cubeOut(float t)
	{
		return 1 + (--t) * t * t;
	}

	public static float cubeInOut(float t)
	{
		return t <= .5 ? t * t * t * 4 : 1 + (--t) * t * t * 4;
	}

	public static float quartIn(float t)
	{
		return t * t * t * t;
	}

	public static float quartOut(float t)
	{
		return 1 - (t -= 1) * t * t * t;
	}

	public static float quartInOut(float t)
	{
		return t <= .5f ? t * t * t * t * 8 : (1 - (t = t * 2 - 2) * t * t * t) / 2 + .5f;
	}

	public static float quintIn(float t)
	{
		return t * t * t * t * t;
	}

	public static float quintOut(float t)
	{
		return (t = t - 1) * t * t * t * t + 1;
	}

	public static float quintInOut(float t)
	{
		return ((t *= 2) < 1) ? (t * t * t * t * t) / 2 : ((t -= 2) * t * t * t * t + 2) / 2;
	}

	/** @since 4.3.0 */
	public static float smoothStepIn(float t)
	{
		return 2 * smoothStepInOut(t / 2);
	}

	/** @since 4.3.0 */
	public static float smoothStepOut(float t)
	{
		return 2 * smoothStepInOut(t / 2 + 0.5f) - 1;
	}

	/** @since 4.3.0 */
	public static float smoothStepInOut(float t)
	{
		return t * t * (t * -2 + 3);
	}

	/** @since 4.3.0 */
	public static float smootherStepIn(float t)
	{
		return 2 * smootherStepInOut(t / 2);
	}

	/** @since 4.3.0 */
	public static float smootherStepOut(float t)
	{
		return 2 * smootherStepInOut(t / 2 + 0.5f) - 1;
	}

	/** @since 4.3.0 */
	public static float smootherStepInOut(float t)
	{
		return t * t * t * (t * (t * 6 - 15) + 10);
	}

	public static float sineIn(float t)
	{
		return -Mathf.Cos(PI2 * t) + 1;
	}

	public static float sineOut(float t)
	{
		return Mathf.Sin(PI2 * t);
	}

	public static float sineInOut(float t)
	{
		return -Mathf.Cos((float)Math.PI * t) / 2 + .5f;
	}

	public static float bounceIn(float t)
	{
		return 1 - bounceOut(1 - t);
	}

	public static float bounceOut(float t)
	{
		if (t < B1)
			return 7.5625f * t * t;
		if (t < B2)
			return 7.5625f * (t - B3) * (t - B3) + .75f;
		if (t < B4)
			return 7.5625f * (t - B5) * (t - B5) + .9375f;
		return 7.5625f * (t - B6) * (t - B6) + .984375f;
	}

	public static float bounceInOut(float t)
	{
		return t < 0.5
			? (1 - bounceOut(1 - 2 * t)) / 2
			: (1 + bounceOut(2 * t - 1)) / 2;
	}

	public static float circIn(float t)
	{
		return -(Mathf.Sqrt(1 - t * t) - 1);
	}

	public static float circOut(float t)
	{
		return Mathf.Sqrt(1 - (t - 1) * (t - 1));
	}

	public static float circInOut(float t)
	{
		return t <= .5 ? (Mathf.Sqrt(1 - t * t * 4) - 1) / -2 : (Mathf.Sqrt(1 - (t * 2 - 2) * (t * 2 - 2)) + 1) / 2;
	}

	public static float expoIn(float t)
	{
		return Mathf.Pow(2, 10 * (t - 1));
	}

	public static float expoOut(float t)
	{
		return -Mathf.Pow(2, -10 * t) + 1;
	}

	public static float expoInOut(float t)
	{
		return t < .5 ? Mathf.Pow(2, 10 * (t * 2 - 1)) / 2 : (-Mathf.Pow(2, -10 * (t * 2 - 1)) + 2) / 2;
	}

	public static float backIn(float t)
	{
		return t * t * (2.70158f * t - 1.70158f);
	}

	public static float backOut(float t)
	{
		return 1 - (--t) * (t) * (-2.70158f * t - 1.70158f);
	}

	public static float backInOut(float t)
	{
		t *= 2;
		if (t < 1)
			return t * t * (2.70158f * t - 1.70158f) / 2;
		t--;
		return (1 - (--t) * (t) * (-2.70158f * t - 1.70158f)) / 2 + .5f;
	}

	public static float elasticIn(float t)
	{
		return -(ELASTIC_AMPLITUDE * Mathf.Pow(2,
			10 * (t -= 1)) * Mathf.Sin((t - (ELASTIC_PERIOD / (2 * (float)Math.PI) * Mathf.Asin(1 / ELASTIC_AMPLITUDE))) * (2 * (float)Math.PI) / ELASTIC_PERIOD));
	}

	public static float elasticOut(float t)
	{
		return (ELASTIC_AMPLITUDE * Mathf.Pow(2,
			-10 * t) * Mathf.Sin((t - (ELASTIC_PERIOD / (2 * (float)Math.PI) * Mathf.Asin(1 / ELASTIC_AMPLITUDE))) * (2 * (float)Math.PI) / ELASTIC_PERIOD)
			+ 1);
	}

	public static float elasticInOut(float t)
	{
		if (t < 0.5)
		{
			return -0.5f * (Mathf.Pow(2, 10 * (t -= 0.5f)) * Mathf.Sin((t - (ELASTIC_PERIOD / 4)) * (2 * (float)Math.PI) / ELASTIC_PERIOD));
		}
		return Mathf.Pow(2, -10 * (t -= 0.5f)) * Mathf.Sin((t - (ELASTIC_PERIOD / 4)) * (2 * (float)Math.PI) / ELASTIC_PERIOD) * 0.5f + 1;
	}
}