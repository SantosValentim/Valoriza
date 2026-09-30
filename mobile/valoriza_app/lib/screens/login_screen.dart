import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import 'home_shell.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _email = TextEditingController();
  final _senha = TextEditingController();
  bool _loading = false;
  String? _erro;

  Future<void> _entrar() async {
    setState(() {
      _loading = true;
      _erro = null;
    });
    final ok = await AuthService().login(_email.text.trim(), _senha.text);
    setState(() => _loading = false);
    if (!mounted) return;
    if (ok) {
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const HomeShell()),
      );
    } else {
      setState(() => _erro = 'Credenciais inválidas');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Text('Valoriza', style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold)),
              const SizedBox(height: 32),
              TextField(controller: _email, decoration: const InputDecoration(labelText: 'E-mail'), keyboardType: TextInputType.emailAddress),
              const SizedBox(height: 12),
              TextField(controller: _senha, decoration: const InputDecoration(labelText: 'Senha'), obscureText: true),
              if (_erro != null) ...[
                const SizedBox(height: 12),
                Text(_erro!, style: const TextStyle(color: Colors.red)),
              ],
              const SizedBox(height: 24),
              FilledButton(
                onPressed: _loading ? null : _entrar,
                child: _loading ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2)) : const Text('Entrar'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}