/* ============================================================
   Serviço de Autenticação
   ============================================================ */

import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;

class AuthService {
  // Altere para o endereço real da API
  static const String baseUrl = 'https://10.0.2.2:7001/api';

  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  Future<Map<String, dynamic>> login(String email, String senha) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'senha': senha}),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      final token = data['dados']?['token'] ?? data['token'];
      await _storage.write(key: 'jwt_token', value: token);
      await _storage.write(key: 'usuario', value: jsonEncode(data['dados']?['usuario'] ?? data['usuario']));
      return data;
    } else {
      throw Exception('Credenciais inválidas');
    }
  }

  Future<String?> getToken() async => await _storage.read(key: 'jwt_token');

  Future<void> logout() async {
    await _storage.delete(key: 'jwt_token');
    await _storage.delete(key: 'usuario');
  }

  Future<bool> isAuthenticated() async {
    final token = await getToken();
    return token != null && token.isNotEmpty;
  }
}
