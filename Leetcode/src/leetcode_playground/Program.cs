using System;
using System.Linq;

Console.WriteLine("Hello from leetcode_playground!");
Console.WriteLine($"Arguments: {string.Join(' ', args)}");

int[] nums = { 1, 2, 1 };
// concatenate nums with itself
int n = nums.Length;
int[] ans = new int[n * 2];
Array.Copy(nums, 0, ans, 0, n);
Array.Copy(nums, 0, ans, n, n);

for (int i = 0; i < ans.Length; i++)
{
    Console.WriteLine(ans[i]);
}