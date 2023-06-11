using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography.Xml;
using System.Collections.Generic;

namespace HotelAPI2.Repositories
{
	public class ReservationRepository
	{
		
		public Response<ReservationsOutput> AddReservation(Reservation res, HotelContext hc, Mappers mappers)
		{
			Response<ReservationsOutput> result = new Response<ReservationsOutput>();
			
			hc.Reservations.Add(res);
			if (hc.SaveChangesAsync().Result != 0)
			{
				result.Success = mappers.mapReservation(res);
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}


		public Response<ReservationsOutput> GetOne(int id, HotelContext hc, Mappers mappers)
		{
			Response<ReservationsOutput> result = new Response<ReservationsOutput>();
			var reservation = hc.Reservations.Include("Clients").Include("User").Where(x => x.Id == id).FirstOrDefaultAsync().Result;
			if (reservation is Reservation)
			{
				result.Success = mappers.mapReservation(reservation);
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public Response<bool> Remove(int id, HotelContext hc)
		{
			Response<bool> result = new Response<bool>();
			var reservation = hc.Reservations.FindAsync(id).Result;
			if (reservation is Reservation)
			{
				hc.Reservations.Remove(reservation);

				if (hc.SaveChangesAsync().Result != 0)
				{
					result.Success = true;
				}
				else
				{
					result.Error = true;
					result.Message = "Error in operation";
				}

			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}

		public Response<ReservationsOutput> Edit(Reservation res, int id,string email, HotelContext hc, Mappers mappers)
		{
			Response<ReservationsOutput> result = new Response<ReservationsOutput>();
			var reservation = hc.Reservations.Include("Clients").Include("User").Where(x=>x.Id==id).FirstOrDefaultAsync().Result;
			if (reservation is Reservation)
			{
				reservation.AdvanceManagement = res.AdvanceManagement;
				reservation.DateIn = res.DateIn;
				reservation.DateOut = res.DateOut;
				reservation.Description = res.Description;
				reservation.Discount = res.Discount;
				reservation.Management = res.Management;
				reservation.NoClients = res.NoClients;
				reservation.CostPerClient = res.CostPerClient;
				reservation.Transport = res.Transport;
				reservation.TotalNights = res.TotalNights;
				reservation.PaymentNights = res.PaymentNights;
				reservation.UpdatedTime = DateTime.UtcNow;
				reservation.UpdatedBy = email;

				if (hc.SaveChangesAsync().Result != 0)
				{

					result.Success = mappers.mapReservation(reservation);
				}
				else
				{
					result.Error = true;
					result.Message = "Error in operation";
				}

			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}

		public Response<List<ReservationsOutput>> GetAll(HotelContext hc, Mappers mappers, LoggedInUser user)
		{
			Response<List<ReservationsOutput>> result = new Response<List<ReservationsOutput>>();
			var reservations = new List<Reservation>();
			if (user.Role == "admin")
			{
				reservations = hc.Reservations.Include("Clients").Include("User").ToListAsync().Result;
			}else if(user.Role == "manager")
			{
				reservations = hc.Reservations.Where(x=>x.User.Email==user.Email).Include("Clients").Include("User").ToListAsync().Result;
			}

			if (reservations is List<Reservation>)
			{
				List<ReservationsOutput> reservarionOutput = new List<ReservationsOutput>();
				foreach (Reservation reservation in reservations)
				{
					reservarionOutput.Add(mappers.mapReservation(reservation));
				}
				result.Success = reservarionOutput;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public Response<List<ReservationsOutput>> GetCurrentReservations(HotelContext hc, Mappers mappers)
		{
			Response<List<ReservationsOutput>> result = new Response<List<ReservationsOutput>>();


			var currentDate = DateTime.Today;

			var reservations = hc.Reservations.Include("Clients").Include("User").Where(reservation => reservation.DateOut.Date >= currentDate).ToListAsync().Result;


			if (reservations is List<Reservation>)
			{
				List<ReservationsOutput> reservarionOutput = new List<ReservationsOutput>();
				foreach (Reservation reservation in reservations)
				{
					reservarionOutput.Add(mappers.mapReservation(reservation));
				}
				result.Success = reservarionOutput;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}



	}

	
}
