using DinkToPdf;
using DinkToPdf.Contracts;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;

public class PDFGenerator
{
	public string GenerateHtmlContent(ReservationsOutput reservation)
	{
		// Genera el contenido HTML del PDF utilizando una plantilla o código HTML
		// Puedes usar una librería de plantillas como RazorEngine o generar el HTML manualmente

		return $@"
            <html>
            <body>
                <h1>Información de la reserva</h1>
                <p>ID de reserva: {reservation.Id}</p>
                <p>Nombre del cliente: {reservation.UserEmail}</p>
                <!-- Agrega más información de la reserva según tus necesidades -->
            </body>
            </html>";
	}
}