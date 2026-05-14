#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEngine;

namespace Dev.jeon.Editor.Utility
{
    /// <summary>
    /// CSV 파싱 공통 유틸리티
    /// </summary>
    public static class CsvParser
    {
        /// <summary>
        /// CSV 파일 읽기 (UTF-8)
        /// </summary>
        public static string[] ReadLines(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[CsvParser] 파일 없음: {path}");
                return null;
            }
            return File.ReadAllLines(path, Encoding.UTF8);
        }

        /// <summary>
        /// "1,800" 같은 숫자 파싱
        /// </summary>
        public static float ParseFloat(string raw)
        {
            string cleaned = raw.Trim().Replace(",", "");
            return float.TryParse(cleaned, out float result) ? result : 0f;
        }

        /// <summary>
        /// int 파싱 (실패 시 기본값 반환)
        /// </summary>
        public static int ParseInt(string raw, int defaultValue = 0)
        {
            string cleaned = raw.Trim().Replace(",", "");
            return int.TryParse(cleaned, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// 경로 존재 확인 및 없으면 생성
        /// </summary>
        public static bool EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[CsvParser] 폴더 생성: {path}");
            }
            return true;
        }
    }
}
#endif