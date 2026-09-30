import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'api_service.dart';

class AuthService {
  static const _tokenKey = 'jwt';
  static const _userKey = 'user_json';

  Future<bool> login(String email, String senha) async {
    final res = await http.post(
      Uri.parse('${ApiService.baseUrl}/Auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email.trim(), 'senha': senha.trim()}),
    );
    if (res.statusCode != 200) return false;

    final body = jsonDecode(res.body);
    final dados = body['dados'] ?? body;
    final token = dados['token'] as String?;
    final usuario = dados['usuario'];
    if (token == null) return false;

    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
    await prefs.setString(_userKey, jsonEncode(usuario));
    return true;
  }

  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
    await prefs.remove(_userKey);
  }

  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  Future<Map<String, dynamic>?> getUser() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_userKey);
    if (raw == null) return null;
    return jsonDecode(raw) as Map<String, dynamic>;
  }

  Future<List<String>> getRoles() async {
    final u = await getUser();
    if (u == null) return [];
    final roles = u['roles'];
    if (roles is List) return roles.map((e) => e.toString()).toList();
    return [];
  }

  Future<bool> isGestorOuAdmin() async {
    final r = await getRoles();
    return r.contains('AdminValoriza') ||
        r.contains('AdminEmpresa') ||
        r.contains('GestorDEI');
  }

  Future<bool> isGestorDEI() async {
    final r = await getRoles();
    return r.contains('GestorDEI');
  }
}