/* ============================================================
   Serviço de comunicação com a API REST
   ============================================================ */

import 'dart:convert';
import 'package:http/http.dart' as http;
import 'auth_service.dart';

class ApiService {
  static const String baseUrl = 'https://10.0.2.2:7001/api';
  final AuthService _authService = AuthService();

  Future<Map<String, String>> _headers() async {
    final token = await _authService.getToken();
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    };
  }

  Future<List<dynamic>> getTrilhas() async {
    final response = await http.get(
      Uri.parse('$baseUrl/treinamentos'),
      headers: await _headers(),
    );
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return data['dados'] ?? data;
    }
    throw Exception('Erro ao carregar trilhas');
  }

  Future<void> enviarDenuncia({
    required String tipo,
    required String relato,
    required bool anonima,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/denuncias'),
      headers: await _headers(),
      body: jsonEncode({
        'tipo': tipo,
        'relato': relato,
        'anonima': anonima,
      }),
    );
    if (response.statusCode != 201 && response.statusCode != 200) {
      throw Exception('Erro ao registrar denúncia');
    }
  }
}
