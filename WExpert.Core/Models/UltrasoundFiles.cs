using System.ComponentModel.DataAnnotations;

namespace WExpert.Database.Models;
public class UltrasoundFiles
{
    [Key]
    public int UltrasoundFileId
    {
        get; set;
    }

    public int PatientId
    {
        get; set;
    }

    public string Path
    {
        get; set;
    } = string.Empty;

    public UltrasoundFiles(int ultrasoundFileId, int patientId, string path)
    {
        UltrasoundFileId = ultrasoundFileId;
        PatientId = patientId;
        Path = path;
    }

    public UltrasoundFiles(int patientId, string path)
    {
        PatientId = patientId;
        Path = path;
    }
}