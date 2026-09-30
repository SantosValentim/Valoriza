import 'package:flutter/material.dart';
import '../services/auth_service.dart';

class PerfilScreen extends StatefulWidget {
  const PerfilScreen({super.key});

  @override
  State<PerfilScreen> createState() => _PerfilScreenState();
}

class _PerfilScreenState extends State<PerfilScreen> {
  Map<String, dynamic>? _user;

  @override
  void initState() {
    super.initState();
    AuthService().getUser().then((u) => setState(() => _user = u));
  }

  String _iniciais(String nome) {
    final parts = nome.trim().split(RegExp(r'\s+'));
    if (parts.isEmpty || parts.first.isEmpty) return '?';
    if (parts.length == 1) return parts.first[0].toUpperCase();
    return '${parts.first[0]}${parts[1][0]}'.toUpperCase();
  }

  @override
  Widget build(BuildContext context) {
    if (_user == null) {
      return const Center(child: CircularProgressIndicator());
    }

    final nome = (_user!['nomeSocial'] ?? _user!['nomeCompleto'] ?? 'Usuário').toString();
    final email = (_user!['email'] ?? '').toString();
    final roles = (_user!['roles'] as List?)?.join(' · ') ?? '';
    final empresa = _user!['empresaNome']?.toString();

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: const Color(0xFF1a1a2e),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Column(
            children: [
              CircleAvatar(
                radius: 36,
                backgroundColor: Colors.white24,
                child: Text(
                  _iniciais(nome),
                  style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold),
                ),
              ),
              const SizedBox(height: 12),
              Text(nome, style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w600)),
              const SizedBox(height: 4),
              Text(email, style: TextStyle(color: Colors.white.withValues(alpha: 0.75))),
              if (roles.isNotEmpty) ...[
                const SizedBox(height: 8),
                Text(roles, style: TextStyle(color: Colors.white.withValues(alpha: 0.6), fontSize: 12)),
              ],
            ],
          ),
        ),
        const SizedBox(height: 16),
        Card(
          elevation: 0,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          child: Column(
            children: [
              ListTile(
                leading: const Icon(Icons.badge_outlined),
                title: const Text('Nome'),
                subtitle: Text(nome),
              ),
              const Divider(height: 1),
              ListTile(
                leading: const Icon(Icons.email_outlined),
                title: const Text('E-mail'),
                subtitle: Text(email),
              ),
              if (empresa != null && empresa.isNotEmpty) ...[
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.business_outlined),
                  title: const Text('Empresa'),
                  subtitle: Text(empresa),
                ),
              ],
            ],
          ),
        ),
        const SizedBox(height: 12),
        Text(
          'Para alterar senha ou nome social, use o painel web (menu do avatar).',
          style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
          textAlign: TextAlign.center,
        ),
      ],
    );
  }
}