import 'dart:io';
import 'package:flutter/material.dart';
import 'screens/login_screen.dart';
import 'screens/home_shell.dart';
import 'services/auth_service.dart';

void main() {
  HttpOverrides.global = MyHttpOverrides();
  runApp(const ValorizaApp());
}

class MyHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback = (X509Certificate cert, String host, int port) => true;
  }
}

class ValorizaApp extends StatelessWidget {
  const ValorizaApp({super.key});
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Valoriza',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF1a1a2e)), useMaterial3: true),
      home: const SplashGate(),
    );
  }
}

class SplashGate extends StatefulWidget {
  const SplashGate({super.key});
  @override
  State<SplashGate> createState() => _SplashGateState();
}

class _SplashGateState extends State<SplashGate> {
  @override
  void initState() {
    super.initState();
    AuthService().getToken().then((t) {
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => t != null ? const HomeShell() : const LoginScreen()),
      );
    });
  }

  @override
  Widget build(BuildContext context) => const Scaffold(body: Center(child: CircularProgressIndicator()));
}