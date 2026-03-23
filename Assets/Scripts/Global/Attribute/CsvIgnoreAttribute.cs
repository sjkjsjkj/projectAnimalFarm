using System;

/// <summary>
/// CsvExporterBySO가 리플렉션으로 변수에 해당 어트리뷰트가 붙었는지 판별하기 위한 상태
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)] // 제약 조건
public sealed class CsvIgnoreAttribute : Attribute
{
    // Field : 변수에만 이 클래스를 붙이도록 강제한다.
    // Multiple : 한 변수에 이 클래스를 중복으로 붙이지 못하도록 한다.
}
