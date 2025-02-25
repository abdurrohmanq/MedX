﻿using AutoMapper;
using MedX.Domain.Entities;
using MedX.Data.IRepositories;
using MedX.Service.Extensions;
using MedX.Service.Interfaces;
using MedX.Service.Exceptions;
using MedX.Service.DTOs.Assets;
using MedX.Domain.Configurations;
using MedX.Domain.Entities.Assets;
using MedX.Service.DTOs.Employees;
using Microsoft.EntityFrameworkCore;
using MedX.Domain.Enums;

namespace MedX.Service.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IRepository<Employee> employeeRepository;
    private readonly IAssetService assetService;
    private readonly IMapper mapper;
    public EmployeeService(IMapper mapper, IRepository<Employee> employeeRepository, IAssetService assetService)
    {
        this.mapper = mapper;
        this.assetService = assetService;
        this.employeeRepository = employeeRepository;
    }
    public async Task<EmployeeResultDto> AddAsync(EmployeeCreationDto dto)
    {
        var existDoctor = await this.employeeRepository.GetAsync(d => d.Phone.Equals(dto.Phone));
        if (existDoctor is not null)
            throw new AlreadyExistException($"This doctor already exist with phone: {dto.Phone}");

        string accountNumber = GenerateAccountNumber();
        var mappedDoctor = new Employee()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            Email = dto.Email,
            Phone = dto.Phone,
            Professional = dto.Professional,
            AccountNumber = accountNumber,
            Salary = dto.Salary,
            Percentage = dto.Percentage,
        };

        if (dto.Image is not null)
        {
            var uploadedImage = await this.assetService.UploadAsync(new AssetCreationDto { FormFile = dto.Image });
            var createdImage = new Asset
            {
                FileName = uploadedImage.FileName,
                FilePath = uploadedImage.FilePath,
            };

            mappedDoctor.ImageId = uploadedImage.Id;
            mappedDoctor.Image = createdImage;
        }

        await this.employeeRepository.CreateAsync(mappedDoctor);
        await this.employeeRepository.SaveChanges();

        return this.mapper.Map<EmployeeResultDto>(mappedDoctor);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var existDoctor = await this.employeeRepository.GetAsync(r => r.Id == id)
            ?? throw new NotFoundException($"This doctor not found with id: {id}");

        this.employeeRepository.Delete(existDoctor);
        await this.assetService.RemoveAsync(existDoctor.Image);
        await this.employeeRepository.SaveChanges();

        return true;
    }
    public async Task<EmployeeResultDto> UpdateAsync(EmployeeUpdateDto dto)
    {
        var existDoctor = await this.employeeRepository.GetAsync(r => r.Id.Equals(dto.Id))
            ?? throw new NotFoundException($"This doctor not found with id: {dto.Id}");

        var uploadedImage = new Asset();
        if (dto.Image is not null)
        {
            uploadedImage = await this.assetService.UploadAsync(new AssetCreationDto { FormFile = dto.Image });
            await this.assetService.RemoveAsync(existDoctor.Image);
        }

        existDoctor.FirstName = dto.FirstName;
        existDoctor.LastName = dto.LastName;
        existDoctor.Patronymic = dto.Patronymic;
        existDoctor.Email = dto.Email;
        existDoctor.Phone = dto.Phone;
        existDoctor.Professional = dto.Professional;
        existDoctor.Salary = dto.Salary;
        existDoctor.Percentage = dto.Percentage;

        if (uploadedImage.Id > 0)
        {
            if (existDoctor.Image == null)
            {
                existDoctor.Image = new Asset();
            }
            existDoctor.ImageId = uploadedImage.Id;
            existDoctor.Image.FileName = uploadedImage.FileName;
            existDoctor.Image.FilePath = uploadedImage.FilePath;
        }

        this.employeeRepository.Update(existDoctor);
        await this.employeeRepository.SaveChanges();

        return this.mapper.Map<EmployeeResultDto>(existDoctor);
    }
    public async Task<EmployeeResultDto> GetAsync(long id)
    {
        var existDoctor = await this.employeeRepository.GetAsync(r => r.Id == id, includes: new[] { "Image" })
            ?? throw new NotFoundException($"This doctor not found with id: {id}");

        return this.mapper.Map<EmployeeResultDto>(existDoctor);
    }

    public async Task<IEnumerable<EmployeeResultDto>> GetAllAsync(PaginationParams @params, string search = null)
    {
        var allEmployees = await this.employeeRepository.GetAll(includes: new[] { "Image" })
            .ToPaginate(@params)
            .ToListAsync();

        if (search is not null)
        {
            search = search.Replace("+", "%2B");
            allEmployees = allEmployees.Where(d => d.Professional.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.Patronymic.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.Phone.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return this.mapper.Map<IEnumerable<EmployeeResultDto>>(allEmployees);
    }


    public async Task<IEnumerable<EmployeeResultDto>> GetAllDoctorAsync(PaginationParams @params, string search = null)
    {
        var allDoctors = await this.employeeRepository.GetAll(d => d.Degree.Equals(Degree.Primary),
             includes: new[] { "Image" })
            .ToPaginate(@params)
            .ToListAsync();

        if (search is not null)
        {
            search = search.Replace("+", "%2B");
            allDoctors = allDoctors.Where(d => d.Professional.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.Patronymic.Contains(search, StringComparison.OrdinalIgnoreCase)
            || d.Phone.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return this.mapper.Map<IEnumerable<EmployeeResultDto>>(allDoctors);
    }
    private string GenerateAccountNumber()
    {
        Random random = new Random();
        int accountNumberLength = 9;
        string accountNumber = random.Next((int)Math.Pow(10,
        accountNumberLength - 1), (int)Math.Pow(10, accountNumberLength)).ToString();

        return accountNumber;
    }
}