/* Comunicação com a API REST Valoriza */

import 'dart:convert';
import 'package:http/http.dart' as http;
import 'auth_service.dart';

class ApiService {
  // Emulador Android → 10.0.2.2 = localhost do PC
  static const String baseUrl = 'http://127.0.0.1:5000/api';

  final AuthService _authService = AuthService();

  Future<Map<String, String>> _headers() async {
    final token = await _authService.getToken();
    return {
      'Content-Type': 'application/json',
      if (token != null && token.isNotEmpty) 'Authorization': 'Bearer $token',
    };
  }

  dynamic _dados(http.Response res) {
    final body = jsonDecode(res.body);
    if (body is Map && body.containsKey('dados')) return body['dados'];
    return body;
  }

  // genéricos
  Future<dynamic> get(String path) async {
    final res = await http.get(Uri.parse('$baseUrl$path'), headers: await _headers());
    if (res.statusCode >= 200 && res.statusCode < 300) return _dados(res);
    throw Exception('GET $path → ${res.statusCode}');
  }

  Future<dynamic> post(String path, Map<String, dynamic> body) async {
    final res = await http.post(
      Uri.parse('$baseUrl$path'),
      headers: await _headers(),
      body: jsonEncode(body),
    );
    if (res.statusCode >= 200 && res.statusCode < 300) return _dados(res);
    throw Exception('POST $path → ${res.statusCode} ${res.body}');
  }

  Future<dynamic> put(String path, Map<String, dynamic> body) async {
    final res = await http.put(
      Uri.parse('$baseUrl$path'),
      headers: await _headers(),
      body: jsonEncode(body),
    );
    if (res.statusCode >= 200 && res.statusCode < 300) return _dados(res);
    throw Exception('PUT $path → ${res.statusCode}');
  }

  // treinamentos
  Future<List<dynamic>> getTrilhas() async {
    final data = await get('/Treinamentos');
    if (data is List) return data;
    if (data is Map && data['dados'] is List) return data['dados'];
    return [];
  }

  Future<Map<String, dynamic>> getTrilha(int id) async {
    final data = await get('/Treinamentos/$id');
    return Map<String, dynamic>.from(data as Map);
  }

  Future<void> concluirConteudo(int conteudoId, {String? respostaQuiz}) async {
    await post('/Treinamentos/conteudos/$conteudoId/concluir', {
      if (respostaQuiz != null) 'resposta': respostaQuiz,
  });
  }

  // denúncias
  Future<List<dynamic>> getDenuncias() async {
    final data = await get('/Denuncias');
    if (data is List) return data;
    if (data is Map && data['dados'] is List) return data['dados'];
    return [];
  }

  Future<void> enviarDenuncia({
    String? titulo,
    required String descricao,
    String? categoria,
    required bool anonima,
  }) async {
    await post('/Denuncias', {
      'titulo': titulo,
      'descricao': descricao,
      'categoria': categoria,
      'anonima': anonima,
    });
  }

  // indicadores
  Future<List<dynamic>> getIndicadores() async {
    final data = await get('/Indicadores');
    if (data is List) return data;
    if (data is Map && data['dados'] is List) return data['dados'];
    return [];
  }

  // mentorias
  Future<List<dynamic>> getMentorias() async {
    final data = await get('/Mentorias');
    if (data is List) return data;
    if (data is Map && data['dados'] is List) return data['dados'];
    return [];
  }
}