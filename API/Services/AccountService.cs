﻿using System.Security.Claims;
using API.Contracts;
using API.DTOS.Accounts;
using API.Models;
using API.Utilities;

namespace API.Services
{
    public class AccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUniversityRepository _universityRepository;
        private readonly IEducationRepository _educationRepository;
        private readonly ITokenHandler _tokenHandler;

        public AccountService(IAccountRepository accountRepository,
                         IEmployeeRepository employeeRepository,
                         IUniversityRepository universityRepository,
                         IEducationRepository educationRepository,
                         ITokenHandler tokenHandler)
        {
            _accountRepository = accountRepository;
            _employeeRepository = employeeRepository;
            _universityRepository = universityRepository;
            _educationRepository = educationRepository;
            _tokenHandler = tokenHandler;
        }

        public IEnumerable<GetAccountsDto>? GetAccount()
        {
            var accounts = _accountRepository.GetAll();
            if (!accounts.Any())
            {
                return null; // No Account  found
            }
            var toDto = accounts.Select(account =>
                                                new GetAccountsDto
                                                {
                                                    guid = account.Guid,
                                                    Password = account.Password,
                                                    IsDeleted = account.IsDeleted,
                                                    IsUsed = account.IsUsed,
                                                }).ToList();

            return toDto; // Account found
        }

        public GetAccountsDto? GetAccountByGuid(Guid guid)
        {
            var account = _accountRepository.GetByGuid(guid);
            if (account is null)
            {
                return null; // account not found
            }

            var toDto = new GetAccountsDto
            {
                guid = account.Guid,
                Password = account.Password,
                IsDeleted = account.IsDeleted,
                IsUsed = account.IsUsed,
            };
            return toDto; // accounts found
        }

        public GetAccountsDto? CreateAccount(NewAccountsDto newAccountDto)
        {
            var account = new Account
            {
                Guid = newAccountDto.Guid,
                Password = Hashing.HashPassword(newAccountDto.Password),
                OTP = newAccountDto.OTP,
                IsUsed = newAccountDto.IsUsed,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,

            };

            var createdAccount = _accountRepository.Create(account);
            if (createdAccount is null)
            {
                return null; // Account not created
            }

            var toDto = new GetAccountsDto
            {
                guid = createdAccount.Guid,
                Password = createdAccount.Password,
                IsDeleted = createdAccount.IsDeleted,
                IsUsed = createdAccount.IsUsed,
            };
            return toDto; // Account created
        }

        public int UpdateAccount(UpdateAccountsDto updateAccountDto)
        {
            var isExist = _accountRepository.IsExist(updateAccountDto.Guid);
            if (!isExist)
            {
                return -1; // Account not found
            }

            var getAccount = _accountRepository.GetByGuid(updateAccountDto.Guid);

            var account = new Account
            {
                Guid = updateAccountDto.Guid,
                IsUsed = updateAccountDto.IsUsed,
                Password = Hashing.HashPassword(updateAccountDto.Password),
                IsDeleted = updateAccountDto.IsDeleted,
                ModifiedDate = DateTime.Now,
                CreatedDate = getAccount!.CreatedDate
            };

            var isUpdate = _accountRepository.Update(account);
            if (!isUpdate)
            {
                return 0; // Account not updated
            }

            return 1;
        }

        public int DeleteAccount(Guid guid)
        {
            var isExist = _accountRepository.IsExist(guid);
            if (!isExist)
            {
                return -1; // Account not found
            }

            var account = _accountRepository.GetByGuid(guid);
            var isDelete = _accountRepository.Delete(account!);
            if (!isDelete)
            {
                return 0; // Account not deleted
            }

            return 1;
        }

        public RegisterDto? Register(RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return null;
            }

            EmployeeService employeeService = new EmployeeService(_employeeRepository);
            Employee employee = new Employee
            {
                Guid = new Guid(),
                NIK = employeeService.GenerateNik(),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                BirthDate = registerDto.BirthDate,
                Gender = registerDto.Gender,
                HiringDate = registerDto.HiringDate,
                Email = registerDto.Email,
                PhoneNumber = registerDto.Phone,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            var createdEmployee = _employeeRepository.Create(employee);
            if (createdEmployee is null)
            {
                return null;
            }

            University university = new University
            {
                Guid = new Guid(),
                Code = registerDto.UniversityCode,
                Name = registerDto.UniversityName
            };

            var createdUniversity = _universityRepository.Create(university);
            if (createdUniversity is null)
            {
                return null;
            }

            Education education = new Education
            {
                Guid = employee.Guid,
                Major = registerDto.Major,
                Degree = registerDto.Degree,
                GPA = registerDto.Gpa,
                UniversityGuid = university.Guid
            };

            var createdEducation = _educationRepository.Create(education);
            if (createdEducation is null)
            {
                return null;
            }

            Account account = new Account
            {
                Guid = employee.Guid,
                Password = Hashing.HashPassword(registerDto.Password),
            };

            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return null;
            }

            var createdAccount = _accountRepository.Create(account);
            if (createdAccount is null)
            {
                return null;
            }


            var toDto = new RegisterDto
            {
                FirstName = createdEmployee.FirstName,
                LastName = createdEmployee.LastName,
                BirthDate = createdEmployee.BirthDate,
                Gender = createdEmployee.Gender,
                HiringDate = createdEmployee.HiringDate,
                Email = createdEmployee.Email,
                Phone = createdEmployee.PhoneNumber,
                Password = createdAccount.Password,
                Major = createdEducation.Major,
                Degree = createdEducation.Degree,
                Gpa = createdEducation.GPA,
                UniversityCode = createdUniversity.Code,
                UniversityName = createdUniversity.Name
            };

            return toDto;
        }

        public string Login(LoginDto login)
        {
            var emailEmp = _employeeRepository.GetEmail(login.Email);
            if (emailEmp is null)
            {
                return "0";
            }

            var password = _accountRepository.GetByGuid(emailEmp.Guid);
            var isValid = Hashing.ValidatePassword(login.Password, password!.Password);
            if (!isValid)
            {
                return "-1";
            }

            var claims = new List<Claim>() {
                new Claim("NIK", emailEmp.NIK),
                new Claim("FullName", $"{emailEmp.FirstName} {emailEmp.LastName}"),
                new Claim("Email", emailEmp.NIK)
            };

            try
            {
                var getToken = _tokenHandler.GenerateToken(claims);
                return getToken;
            }
            catch
            {
                return "-2";
            }

        }


    }
}

