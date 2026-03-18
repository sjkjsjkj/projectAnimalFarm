using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 격자를 다루는 유틸리티 클래스입니다.
/// </summary>
public static class UGrid
{
    /// <summary>
    /// 월드 좌표를 격자 좌표로 변환하여 반환합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int WorldToGrid(Vector2 pos)
    {
        pos.x = Mathf.FloorToInt(pos.x);
        pos.y = Mathf.FloorToInt(pos.y);
        return new Vector2Int((int)pos.x, (int)pos.y);
    }


    /// <summary>
    /// 월드 좌표를 격자 좌표로 변환하여 반환합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int x, int y) WorldToGrid(float x, float y)
    {
        int nX = Mathf.FloorToInt(x);
        int nY = Mathf.FloorToInt(y);
        return (nX, nY);
    }

    /// <summary>
    /// 월드 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WorldToIndex(Vector2 pos, int width)
    {
        int nX = Mathf.FloorToInt(pos.x);
        int nY = Mathf.FloorToInt(pos.y);
        return GridToIndex(nX, nY, width);
    }

    /// <summary>
    /// 월드 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WorldToIndex(float x, float y, int width)
    {
        x = Mathf.FloorToInt(x);
        y = Mathf.FloorToInt(y);
        return GridToIndex(x, y, width);
    }

    /// <summary>
    /// 격자 좌표를 월드 좌표로 변환하여 중심점을 반환합니다.
    /// </summary>
    /// <param name="pos">격자 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GridToWorld(Vector2Int pos)
    {
        float x = (float)pos.x + K.GRID_SIZE_HALF;
        float y = (float)pos.y + K.GRID_SIZE_HALF;
        return new Vector2(x, y);
    }

    /// <summary>
    /// 격자 좌표를 월드 좌표로 변환하여 중심점을 반환합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float x, float y) GridToWorld(int x, int y)
    {
        return ((float)x + K.GRID_SIZE_HALF, (float)y + K.GRID_SIZE_HALF);
    }

    /// <summary>
    /// 격자 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GridToIndex(int x, int y, int width)
    {
        return (y * width) + x;
    }

    /// <summary>
    /// 월드 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GridToIndex(float x, float y, int width)
    {
        int gridX, gridY;
        (gridX, gridY) = WorldToGrid(x, y);
        return (gridY * width) + gridX;
    }

    /// <summary>
    /// 월드 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GridToIndex(Vector2 pos, int width)
    {
        Vector2Int gridPos = WorldToGrid(pos);
        return (gridPos.y * width) + gridPos.x;
    }

    /// <summary>
    /// 격자 배열에서 사용하는 인덱스를 격자 좌표로 변환합니다.
    /// </summary>
    /// <param name="index">격자 배열 인덱스</param>
    /// <param name="width">맵 가로 길이</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int IndexToGrid(int index, int width)
    {
        int x = index % width;
        int y = index / width;
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 인덱스를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="index">격자 배열 인덱스</param>
    /// <param name="width">맵 가로 길이</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 IndexToWorld(int index, int width)
    {
        Vector2Int gridPos = IndexToGrid(index, width);
        float fX = gridPos.x + K.GRID_SIZE_HALF;
        float fY = gridPos.y + K.GRID_SIZE_HALF;
        return new Vector2(fX, fY);
    }

    /// <summary>
    /// 격자 배열 인덱스가 맵 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="index">격자 배열 인덱스</param>
    /// <param name="width">맵 가로 길이</param>
    /// <param name="height">맵 세로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InMap(int index, int width, int height)
    {
        if (index < 0 || index >= width * height)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 좌표가 맵 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <param name="height">맵 세로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InMap(Vector2 pos, int width, int height)
    {
        if (pos.x < 0f) return false;
        if (pos.y < 0f) return false;
        if ((float)width <= pos.x) return false;
        if ((float)height <= pos.y) return false;
        return true;
    }

    /// <summary>
    /// 월드 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <param name="height">맵 세로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float x, float y) ClampMap(float x, float y, int width, int height)
    {
        x = Mathf.Clamp(x, K.SMALL_DISTANCE, (float)width - K.SMALL_DISTANCE);
        y = Mathf.Clamp(y, K.SMALL_DISTANCE, (float)height - K.SMALL_DISTANCE);
        return (x, y);
    }

    /// <summary>
    /// 격자 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <param name="height">맵 세로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int x, int y) ClampMap(int x, int y, int width, int height)
    {
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);
        return (x, y);
    }

    /// <summary>
    /// 월드 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <param name="height">맵 세로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ClampMap(Vector2 pos, int width, int height)
    {
        pos.x = Mathf.Clamp(pos.x, K.SMALL_DISTANCE, (float)width - K.SMALL_DISTANCE);
        pos.y = Mathf.Clamp(pos.y, K.SMALL_DISTANCE, (float)height - K.SMALL_DISTANCE);
        return pos;
    }

    /// <summary>
    /// 격자 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="pos"> 격자 좌표</param>
    /// <param name="width">맵 가로 길이</param>
    /// <param name="height">맵 세로 길이</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int ClampMap(Vector2Int pos, int width, int height)
    {
        pos.x = Mathf.Clamp(pos.x, 0, width - 1);
        pos.y = Mathf.Clamp(pos.y, 0, height - 1);
        return pos;
    }

    /// <summary>
    /// 유닛이 점유한 격자를 순환할 수 있는 변수를 반환받습니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <param name="size">유닛 크기</param>
    /// <param name="width">맵 가로 길이</param>
    /// <param name="height">맵 세로 길이</param>
    /// <returns></returns>
    public static (int startX, int endX, int startY, int endY)
        GetForeachGrid(Vector2 pos, Vector2 size, int width, int height)
    {
        // 유닛 크기 결정
        float halfX = size.x * K.GRID_SIZE_HALF;
        float halfY = size.y * K.GRID_SIZE_HALF;
        // 점유한 격자 좌표의 시작과 끝
        int startX = (int)(pos.x - halfX);
        int endX = (int)(pos.x + halfX);
        int startY = (int)(pos.y - halfY);
        int endY = (int)(pos.y + halfY);
        (startX, startY) = ClampMap(startX, startY, width, height);
        (endX, endY) = ClampMap(endX, endY, width, height);
        // 완료
        return (startX, endX, startY, endY);
    }

    /// <summary>
    /// 월드 좌표가 특정 격자 좌표 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <param name="gridX">격자 X 좌표</param>
    /// <param name="gridY">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InGrid(float x, float y, int gridX, int gridY)
    {
        float left = (float)gridX;
        float right = (float)gridX + 1f;
        float down = (float)gridY;
        float up = (float)gridY + 1f;
        if (x < left) return false;
        if (y < down) return false;
        if (right <= x) return false;
        if (up <= y) return false;
        return true;
    }

    /// <summary>
    /// 좌표가 특정 격자 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <param name="gridX">격자 X 좌표</param>
    /// <param name="gridY">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InGrid(Vector2 pos, int gridX, int gridY)
    {
        float left = (float)gridX;
        float right = (float)gridX + 1f;
        float down = (float)gridY;
        float up = (float)gridY + 1f;
        if (pos.x < left) return false;
        if (pos.y < down) return false;
        if (right <= pos.x) return false;
        if (up <= pos.y) return false;
        return true;
    }
}
