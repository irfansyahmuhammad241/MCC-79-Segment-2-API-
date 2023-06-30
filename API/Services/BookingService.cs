﻿using API.Contracts;
using API.DTOS.Bookings;
using API.Models;

namespace API.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;

        public BookingService(IBookingRepository bookingRepository, IRoomRepository roomRepository)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
        }

        public IEnumerable<GetBookingsDto>? GetBooking()
        {
            var bookings = _bookingRepository.GetAll();
            if (!bookings.Any())
            {
                return null; // No Booking  found
            }
            var toDto = bookings.Select(booking =>
                                                new GetBookingsDto
                                                {
                                                    Guid = booking.Guid,
                                                    StartDate = booking.StartDate,
                                                    EndDate = booking.EndDate,
                                                    Status = booking.Status,
                                                    Remarks = booking.Remarks,
                                                    RoomGuid = booking.RoomGuid,
                                                    EmployeeGuid = booking.EmployeeGuid
                                                }).ToList();

            return toDto; // Booking found
        }

        public GetBookingsDto? GetBooking(Guid guid)
        {
            var booking = _bookingRepository.GetByGuid(guid);
            if (booking is null)
            {
                return null; // booking not found
            }

            var toDto = new GetBookingsDto
            {
                Guid = booking.Guid,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Status = booking.Status,
                Remarks = booking.Remarks,
                RoomGuid = booking.RoomGuid,
                EmployeeGuid = booking.EmployeeGuid
            };
            return toDto; // bookings found
        }

        public GetBookingsDto? CreateBooking(NewBookingsDto newBookingDto)
        {
            var booking = new Booking
            {
                Guid = new Guid(),
                StartDate = newBookingDto.StartDate,
                EndDate = newBookingDto.EndDate,
                Status = newBookingDto.Status,
                Remarks = newBookingDto.Remarks,
                RoomGuid = newBookingDto.RoomGuid,
                EmployeeGuid = newBookingDto.EmployeeGuid,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            var createdBooking = _bookingRepository.Create(booking);
            if (createdBooking is null)
            {
                return null; // Booking not created
            }

            var toDto = new GetBookingsDto
            {
                Guid = createdBooking.Guid,
                StartDate = newBookingDto.StartDate,
                EndDate = newBookingDto.EndDate,
                Status = newBookingDto.Status,
                Remarks = newBookingDto.Remarks,
                RoomGuid = newBookingDto.RoomGuid,
                EmployeeGuid = newBookingDto.EmployeeGuid,
            };
            return toDto; // Booking created
        }

        public int UpdateBooking(UpdateBookingsDto updateBookingDto)
        {
            var isExist = _bookingRepository.IsExist(updateBookingDto.Guid);
            if (!isExist)
            {
                return -1; // Booking not found
            }

            var getBooking = _bookingRepository.GetByGuid(updateBookingDto.Guid);

            var booking = new Booking
            {
                Guid = updateBookingDto.Guid,
                StartDate = updateBookingDto.StartDate,
                EndDate = updateBookingDto.EndDate,
                Status = updateBookingDto.Status,
                Remarks = updateBookingDto.Remarks,
                RoomGuid = updateBookingDto.RoomGuid,
                EmployeeGuid = updateBookingDto.EmployeeGuid,
                ModifiedDate = DateTime.Now,
                CreatedDate = getBooking!.CreatedDate
            };

            var isUpdate = _bookingRepository.Update(booking);
            if (!isUpdate)
            {
                return 0; // Booking not updated
            }

            return 1;
        }

        public int DeleteBooking(Guid guid)
        {
            var isExist = _bookingRepository.IsExist(guid);
            if (!isExist)
            {
                return -1; // Booking not found
            }

            var booking = _bookingRepository.GetByGuid(guid);
            var isDelete = _bookingRepository.Delete(booking!);
            if (!isDelete)
            {
                return 0; // Booking not deleted
            }

            return 1;
        }

        public IEnumerable<BookingLengthDto>? GetBookingLength()
        {
            var bookings = _bookingRepository.GetAll();
            var rooms = _roomRepository.GetAll();

            var entities = (from booking in bookings
                            join room in rooms on booking.RoomGuid equals room.Guid
                            select new
                            {
                                guid = room.Guid,
                                startDate = booking.StartDate,
                                endDate = booking.EndDate,
                                roomName = room.Name
                            }).ToList();

            var bookingDurations = new List<BookingLengthDto>();

            foreach (var entity in entities)
            {
                var bookingDurationDto = new BookingLengthDto
                {
                    RoomGuid = entity.guid,
                    RoomName = entity.roomName,
                    BookingLength = CalculateLength(entity.startDate, entity.endDate)
                };

                bookingDurations.Add(bookingDurationDto);
            }

            return bookingDurations; // Booking found
        }

        private int CalculateLength(DateTime startDate, DateTime endDate)
        {
            int totalDays = (int)(endDate - startDate).TotalDays + 1;
            int weekends = 0;
            DateTime currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekends++;
                }
                currentDate = currentDate.AddDays(1);
            }

            int bookingLength = totalDays - weekends;
            return bookingLength;
        }
    }


}
