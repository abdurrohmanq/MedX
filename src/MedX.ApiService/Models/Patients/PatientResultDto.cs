﻿using MedX.Domain.Enums;

namespace MedX.ApiService.Models.Patients;

public class PatientResultDto
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Patronymic { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public Gender Gender { get; set; }
    public string Pinfl { get; set; }
    public decimal Balance { get; set; }
    public string AccountNumber { get; set; }
    public AssetResultDto Image { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
