﻿using MedX.Domain.Enums;
using MedX.Service.DTOs.Appointments;
using MedX.Service.DTOs.Assets;
using MedX.Service.DTOs.ServiceItems;
using MedX.Service.DTOs.Treatments;

namespace MedX.Service.DTOs.Patients;

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
    public DateTime CreatedAt { get; set; }
    public AssetResultDto Image { get; set; } = default!;

    public ICollection<TreatmentResultDto> Treatments { get; set; }
    public ICollection<AffairItemResultDto> AffairItems { get; set; }
    public ICollection<AppointmentResultDto> Appointments { get; set; }
}
